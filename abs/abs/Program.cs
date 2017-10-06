using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using monopage;
using System.Globalization;
using System.Security.Cryptography;

namespace abs {
    public class userManager {
        Database db;

        public void addUser(string username, string password, string email) {
            string userid = Guid.NewGuid().ToString();
            string usersalt = Guid.NewGuid().ToString();
            string userpass = password + usersalt;

            addUserFullData(userid, username, email, usersalt, userpass);
        }

        public void addUserFullData(string userid, string username, string email, string usersalt, string userpass) {
            db.addRow("users", new List<string>(new string[] { userid, username, email, usersalt, userpass }));
        }

        public static string md5(byte[] data) {
            MD5 md5 = MD5.Create();//
            byte[] bytes = md5.ComputeHash(data);
            return bytes[0].ToString("X2") + bytes[1].ToString("X2") + bytes[2].ToString("X2") + bytes[3].ToString("X2");
        }

        public userManager(Database db) {
            this.db = db;
        }
    }

    class Program {
        static void Main(string[] args) {
            Database db = new Database("Server=localhost;Port=5432;Username=postgres;Password=admin;Database=postgres");



            planDefinition def = new planDefinition();
            Plan p = new Plan(def, db);
            
            for (int i = 0; i < 50; i++) {
                List<workoutItem> day = p.generateDay(10);
                Console.WriteLine(day.First().uuid + ", " + day.Last().uuid);
                foreach (var item in day) {
                    //Console.WriteLine("\t" + (item.ex.isCompound ? "yes " : "no  ") + item.ex.areaNumber + " " + item.ex.exerciseName);
                    Console.WriteLine(item.readable(500));
                }
                Console.WriteLine();
                Console.WriteLine();
            }



            //refresh users table
            db.deleteTableIfExists("users");
            db.createTableIfNeeded("users", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("userid", "text"),
                new KeyValuePair<string, string>("username", "text"),
                new KeyValuePair<string, string>("email", "text"),
                new KeyValuePair<string, string>("salt", "text"),
                new KeyValuePair<string, string>("password", "text")
            }));

            //initialize userManager
            userManager man = new userManager(db);
            man.addUser("user", "pass", "user@gmail.com");

            //refresh data tables
            db.deleteTableIfExists("weight");
            db.createTableIfNeeded("weight", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("userid", "text"),
                new KeyValuePair<string, string>("date", "text"),
                new KeyValuePair<string, string>("value", "text")
            }));

            for (int i = 0; i < 50; i++) {
                db.addRow("weight", new List<string>(new string[] { "user", DateTime.UtcNow.ToString(), (i * i / 50 + 100).ToString() }));
            }
            





            mpBase mpBase = new mpBase();

            

            Func<Dictionary<string, List<binaryData>>, mpObject> queryResultToJSON = kvpDic => {
                mpObject res = new mpObject();
                if (kvpDic == null) return res;
                foreach(var kvp in kvpDic) {
                    res.addProperty(kvp.Key, new mpArray(kvp.Value.Select(bin => bin.asString()).ToArray()));
                }
                return res;
            };




            mpAuth lman = new mpAuth(db);
            lman.add("weight", authToke => {
                if (authToke.userid != "") {
                    return new mpFunctionalGETableToken(() => {
                        string query = "SELECT date, value FROM weight WHERE userid = '" + authToke.userid + "'";
                        //query = "userid=" + authToke.userid +
                        return new binaryData(queryResultToJSON(db.query(query)).json);
                    });
                } else {
                    return new mpArray();
                }
            });
            mpBase.addProperty("login", lman);//





            mpBase.addProperty("userRecordings", new mpChildGetter(child => {
                return queryResultToJSON(db.query("SELECT date, value FROM " + child + " WHERE userid = 'testid'"));
            }));

            mpPageAssembler assembler = new mpPageAssembler(
                new mpFile("../../Web/Templates/Template.html"),
                new mpFile("../../Web/Templates/Template.css")
            );

            assembler.js.debugMode = true;


            assembler.add("header", new mpFile("../../Web/Header.html"), true, false);
            assembler.add("footer", new mpFile("../../Web/Footer.html"), false, false);
            assembler.add("login", new mpPageElement(new mpFile("../../Web/login.html"), new mpFile("../../Web/login.js")), false, true);
            assembler.add("register", new mpPageElement(new mpFile("../../Web/register.html"), new mpFile("../../Web/register.js")), false, true);
            assembler.add("data", new mpPageElement(new mpFile("../../Web/dataEntry.html"), new mpFile("../../Web/dataEntry.js")), false, true);





            //assembler.add("minmax", new mpPageElement(new mpFile("../../Web/minmax.html"), new mpFile("../../Web/minmax.js")), false, true);




            assembler.add("ss", new mpFunctionalGETableToken((rq) => {
                string htmlRes = "";

                var res = db.query("SELECT date, value FROM weight WHERE userid = '" + lman.checkRequest(rq).userid + "'");
                List<binaryData> list = res["value"];
                double min = 0;
                double max = 0;
                if (list != null && list.Count > 0) {
                    min = res["value"].Select(data => double.Parse(data.asString())).Min();
                    max = res["value"].Select(data => double.Parse(data.asString())).Max();
                }

                Console.WriteLine("id:" + lman.checkRequest(rq).userid);

                htmlRes += "<div class='mpContentContainer'>" +
                            "<div style = 'background-color: burlywood; display: flex; flex-direction: row; align-items: stretch; width: 100%; height: 10vw;'>" +
                                    "<div id = 'min' style = 'text-align: center; line-height: 10vw; width: 100%; font-size: 72pt;' > " + min + " </div>" +
                                    "<div id = 'max' style = 'text-align: center; line-height: 10vw; width: 100%; font-size: 72pt;' > " + max + " </div>" +
                                "</div>" +
                            "</div> ";

                return new mpResponse(new binaryData(htmlRes, binaryDataType.html), 200);
            }), false, true);

            assembler.add("exerciseViewer", new mpPageElement(new mpFile("../../Web/exerciseViewer.html"), new mpFile("../../Web/exerciseViewer.js")), true, false);

            assembler.add("exercisePanels", new mpFunctionalGETableToken((rq) => {
                string workoutItems = "";
                List<workoutItem> items = p.generateDay(8);

                workoutItems += "<div id='exercisesTitle' style='border: 1px solid blue; margin: -1px; width: 20em; height: 2em; text-align: center; line-height: 2em;'>" + "Monday - " + items.First().ex.mainBodyPart + " & " + items.Last().ex.mainBodyPart + "</div>";

                foreach (workoutItem item in items) {
                    workoutItems += item.title(200);
                    //htmlRes += "<br>";
                }

                string itemSections = "";
                foreach(workoutItem item in items) {
                    int index = 0;
                    for(int i = 0; i < item.sets.Count(); i++) {
                        set s = item.sets[i];
                        itemSections += 
                            "<div id='" + item.uuid + "_setinfo_" + (index++) + "' class='mpExerciseSet'>" +
                               s.reps + " reps at " + Util.percent1RM(s.percent1RM, 200) + "lbs" +
                            "</div>";

                        if(i != item.sets.Count() - 1) {
                            if(s.restTime > new TimeSpan(0, 0, 0)) {
                                itemSections +=
                                    "<div id='" + item.uuid + "_setinfo_" + (index++) + "' class='mpExerciseSet'>" +
                                       "Rest " + s.restTime.TotalSeconds + " seconds ... 🕒" +
                                    "</div>";
                            }
                        }
                    }
                }

                string res =
                    "<div id = 'exercises' style = 'border-right: 2px solid #7aa5c2; padding: 1em; margin-right: 1em; display: flex; width: 20em; height: 18em; flex-direction: column;'>" + 
                        workoutItems +
                    "</div>" +
                    "<div id = 'sets' style = 'border-right: 2px solid #7aa5c2; padding: 1em; margin-right: 1em; display: flex; width: 20em; height: 18em; flex-direction: column;'>" +
                        "<div id='setsTitle' style='border: 1px solid blue; margin: -1px; width: 100%; height: 2em; text-align: center; line-height: 2em;'></div>" +
                        itemSections +
                    "</div>" +
                    "<div id = 'sets' style = 'border-right: 2px solid #7aa5c2; padding: 1em; margin-right: 1em; display: flex; width: 20em; height: 18em; flex-direction: column;'>" +
                        "<div id='nextExercise'> Next! </div>" +
                    "</div>";


                return new mpResponse(new binaryData(res, binaryDataType.html), 200);
            }), false, false);




            mpPageAssembler ass = new mpPageAssembler(new mpFile("../../Web/loginEngine.html"), new mpFile(new binaryData("", binaryDataType.css)));
            ass.add("loginItem", new mpFile(new binaryData("document.cookie = \"Authorization=user:pass\";")));
            mpBase.addProperty("loginCookier", ass);

            mpPageAssembler logoutEngine = new mpPageAssembler(new mpFile("../../Web/loginEngine.html"), new mpFile(new binaryData("", binaryDataType.css)));
            logoutEngine.add("loginItem", new mpFile(new binaryData("document.cookie = \"Authorization=none\";")));
            mpBase.addProperty("logoutEngine", logoutEngine);

            mpRestfulTarget loginTestTarget = new mpRestfulTarget(new Func<System.Net.HttpListenerRequest, mpResponse>(
                rq => {
                    if (rq.Cookies["Authorization"].Value == "user:pass") {
                        return new mpResponse("nice", 200);
                    } else {
                        return new mpResponse("not authenticated", 401);
                    }
                }
            ));
            mpBase.addProperty("login", loginTestTarget);
            

            mpRestfulTarget registerTarget = new mpRestfulTarget(null);
            registerTarget.POSTFunc = request => {
                mpToken parsed = mpJson.parse(request.data());
                mpObject rqtoken = parsed as mpObject;
                if (rqtoken is mpObject) {
                    string username = null;
                    string password = null;
                    string email = null;

                    mpToken usernameToke = rqtoken.getChild("username");
                    if (usernameToke != null && usernameToke is mpValue && (usernameToke as mpValue).data.type == binaryDataType.text) {
                        username = (usernameToke as mpValue).data.asString();
                    }
                    mpToken passwordToke = rqtoken.getChild("password");
                    if (passwordToke != null && passwordToke is mpValue && (passwordToke as mpValue).data.type == binaryDataType.text) {
                        password = (passwordToke as mpValue).data.asString();
                    }
                    mpToken emailToke = rqtoken.getChild("email");
                    if (emailToke != null && emailToke is mpValue && (emailToke as mpValue).data.type == binaryDataType.text) {
                        email = (emailToke as mpValue).data.asString();
                    }

                    if (username != null && password != null && email != null) {
                        man.addUser(username, password, email);
                        return mpResponse.empty200();
                    } else {
                        return mpResponse.badRequest();
                    }
                } else {
                    return mpResponse.badRequest();
                }
            };
            mpBase.addProperty("registerTarget", registerTarget);

            
            mpServer server = new mpServer();
            server.start(mpBase.restful, "http://*:8080/");

            mpBase.addProperty("web", assembler);

            Console.ReadKey();

            server.stop();
        }
    }
}
