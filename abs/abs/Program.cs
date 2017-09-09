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
            string userpass = md5((password + usersalt).AsBytes());

            addUserFullData(userid, username, email, usersalt, userpass);
        }

        public void addUserFullData(string userid, string username, string email, string usersalt, string userpass) {
            db.addRow("users", new List<string>(new string[] { userid, username, email, usersalt, userpass }));
        }

        public static string md5(byte[] data) {
            MD5 md5 = MD5.Create();
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
            man.addUserFullData("testid", "testuser", "testemail@gmail.com", "testsalt", "testhash");

            //refresh data tables
            db.deleteTableIfExists("weight");
            db.createTableIfNeeded("weight", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("userid", "text"),
                new KeyValuePair<string, string>("date", "text"),
                new KeyValuePair<string, string>("value", "text")
            }));

            for (int i = 0; i < 50; i++) {
                db.addRow("weight", new List<string>(new string[] { "testid", DateTime.UtcNow.ToString(), (i * i / 50 + 100).ToString() }));
            }

            mpBase mpBase = new mpBase();

            mpBase.addProperty("test", new mpValue("the value at test"));

            mpBase.addProperty("users", new mpQuery(db, "SELECT * FROM users"));

            Func<Dictionary<string, List<binaryData>>, mpObject> queryResultToJSON = kvpDic => {
                mpObject res = new mpObject();
                foreach(var kvp in kvpDic) {
                    res.addProperty(kvp.Key, new mpArray(kvp.Value.Select(bin => bin.asString()).ToArray()));
                }
                return res;
            };

            mpBase.addProperty("userRecordings", new mpChildGetter(child => {
                return queryResultToJSON(db.query("SELECT date, value FROM " + child + " WHERE userid = 'testid'"));
            }));

            mpPageAssembler assembler = new mpPageAssembler(
                new mpFile("../../Web/Templates/Template.html"),
                new mpFile("../../Web/Templates/Template.css")
            );
            assembler.add("header", new mpFile("../../Web/Header.html"), true, false);
            assembler.add("footer", new mpFile("../../Web/Footer.html"), false, false);
            assembler.add("login", new mpPageElement(new mpFile("../../Web/login.html"), new mpFile("../../Web/login.js")), false, true);
            assembler.add("register", new mpPageElement(new mpFile("../../Web/register.html"), new mpFile("../../Web/register.js")), false, true);
            assembler.add("data", new mpPageElement(new mpFile("../../Web/dataEntry.html"), new mpFile("../../Web/dataEntry.js")), false, true);

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
