using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

using monopage;

namespace abs {
    class Abs {
        static void ResetDatabase(Database db) {
            //refresh users table
            db.deleteTableIfExists("users");
            db.createTableIfNeeded("users", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("email", "text"),
                new KeyValuePair<string, string>("salt", "text"),
                new KeyValuePair<string, string>("passwordHashHash", "text")
            }));

            db.deleteTableIfExists("userinfo");
            db.createTableIfNeeded("userinfo", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("email", "text"),
                new KeyValuePair<string, string>("exercise", "text"),
                new KeyValuePair<string, string>("reps", "int"),
                new KeyValuePair<string, string>("weight", "int"),
                new KeyValuePair<string, string>("recorded", "date")
            }));

            db.deleteTableIfExists("workoutdays");
            db.createTableIfNeeded("workoutdays", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("dayid", "text"), //uuid of this day
                new KeyValuePair<string, string>("associateduser", "text"),
                new KeyValuePair<string, string>("workoutdate", "date"),
                new KeyValuePair<string, string>("primarygroup", "text"),
                new KeyValuePair<string, string>("secondarygroup", "text"),
                new KeyValuePair<string, string>("itemcount", "int") //number of exercises
            }));

            db.deleteTableIfExists("workoutitems");
            db.createTableIfNeeded("workoutitems", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("itemid", "text"), //uuid of this item
                new KeyValuePair<string, string>("associatedday", "text"), //uuid of associated day
                new KeyValuePair<string, string>("exercisename", "text"),
                new KeyValuePair<string, string>("setcount", "int"),
                new KeyValuePair<string, string>("feedbackdifficulty", "int")
            }));

            db.deleteTableIfExists("workoutsets");
            db.createTableIfNeeded("workoutsets", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("associateditem", "text"),
                new KeyValuePair<string, string>("reps", "int"),
                new KeyValuePair<string, string>("percent1rm", "int"),
                new KeyValuePair<string, string>("resttime", "real"),
                new KeyValuePair<string, string>("feedbackreps", "int"),
            }));

            db.query("INSERT INTO workoutdays VALUES('1234', 'bob@bob.com', '2018-01-05', 'chest', 'upperchest', 1);");
            db.query("INSERT INTO workoutitems VALUES('5678', '1234', 'Barbell Bench Press ', 1, 0);");
            db.query("INSERT INTO workoutsets VALUES('5678', 10, 75, 30, 9);");
        }

        static void Main(string[] args) {
            string host = "localhost";
            if (args.Length >= 1) {
                host = args[0];
            }
            string connection = "Server=" + host + ";Port=5432;Username=postgres;Password=postpass;Database=postgres";

            Database db = new Database(connection);
            ResetDatabase(db);

            UserManager manager = new UserManager(db);

            HashSet<Exercise> exercises = Exercise.getAllExercises(db);

            Plan p = new Plan(db, new User(db, "bob@bob.com", "salt", Util.hash("password" + "salt")));

            WorkoutDay day = p.generateDay(3);

            mpBase root = new mpBase();

            mpServer server = new mpServer();
            server.start(root.restful, "http://*:8080/");

            #region register
            root.addProperty("register",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            Console.WriteLine("Accout Create Request...");

                            string requestData = req.data();
                            string requestEmail = "", requestPasswordEmailHash = "";

                            try {
                                mpObject requestJSON = (mpObject)mpJson.parse(requestData);

                                requestEmail = ((mpValue)requestJSON.getChild("email")).data.asString();
                                requestPasswordEmailHash = ((mpValue)requestJSON.getChild("passwordEmailHash")).data.asString();

                            } catch (Exception ex) {
                                Console.WriteLine("Account Creation Error:" + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }

                            try {
                                manager.createUser(requestEmail, requestPasswordEmailHash);
                            } catch (Exception ex) {
                                Console.WriteLine("Account Creation Error:" + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }

                            Console.WriteLine("Account Created Correctly");
                            return new mpResponse(new binaryData("{\"good\":true, \"message\":\"Account Created\"}"), 200);
                        }
                    )
                )
            );
            #endregion

            #region login
            root.addProperty("login",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            Console.WriteLine("Login Request...");

                            string requestData = req.data();
                            string requestEmail = "", requestPasswordEmailHash = "";

                            try {
                                mpObject requestJSON = (mpObject)mpJson.parse(requestData);

                                requestEmail = ((mpValue)requestJSON.getChild("email")).data.asString();
                                requestPasswordEmailHash = ((mpValue)requestJSON.getChild("passwordEmailHash")).data.asString();

                            } catch (Exception ex) {
                                Console.WriteLine("Login Error: " + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }

                            try {
                                manager.getUser(requestEmail, requestPasswordEmailHash);
                            } catch (Exception ex) {
                                Console.WriteLine("Login Error: " + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }

                            Console.WriteLine("Login Valid");
                            return new mpResponse(new binaryData("{\"good\":true, \"message\":\"Credentials Valid\"}"), 200);
                        }
                    )
                )
            );
            #endregion

            #region exercise-info
            root.addProperty("exercise-info",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            Console.Write("Exercise Data Requested...");

                            string requestData = req.data();
                            string requestEmail = "", requestPasswordEmailHash = "";
                            int requestNumItems = -1;

                            try {
                                mpObject requestJSON = (mpObject)mpJson.parse(requestData);

                                requestEmail = ((mpValue)requestJSON.getChild("email")).data.asString();
                                requestPasswordEmailHash = ((mpValue)requestJSON.getChild("passwordEmailHash")).data.asString();
                                requestNumItems = ((mpValue)requestJSON.getChild("numItems")).data.asInt();
                            } catch (Exception ex) {
                                Console.WriteLine("Exercise Request Error: " + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }

                            try {
                                User user = manager.getUser(requestEmail, requestPasswordEmailHash);
                                UserInfo info = new UserInfo(db, user);

                                mpResponse res = mpResponse.success();
                                res.response = new binaryData(p.generateDay(requestNumItems).toJSON(info).ToString());

                                Console.WriteLine("Responded! (user = " + requestEmail + ")");
                                return res;
                            } catch (Exception ex) {
                                Console.WriteLine("Exercise Request Error: " + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }
                        }
                    )
                )
            );
            #endregion

            #region exercise-calibration
            root.addProperty("exercise-calibration",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            Console.Write("Received Exercise Calibration Info...");

                            string requestData = req.data();
                            string requestEmail = "", requestPasswordEmailHash = "";
                            int oneRepMax = -1;

                            try {
                                mpObject requestJSON = (mpObject)mpJson.parse(requestData);

                                requestEmail = ((mpValue)requestJSON.getChild("email")).data.asString();
                                requestPasswordEmailHash = ((mpValue)requestJSON.getChild("passwordEmailHash")).data.asString();

                                UserInfo info = new UserInfo(db, manager.getUser(requestEmail, requestPasswordEmailHash));
                                info.AddOneRepMax(
                                    ((mpValue)requestJSON.getChild("exercise")).data.asString(),
                                    ((mpValue)requestJSON.getChild("reps")).data.asInt(),
                                    ((mpValue)requestJSON.getChild("weight")).data.asInt()
                                );
                                info.Store();
                            } catch (Exception ex) {
                                Console.WriteLine("Calibration Info Error: " + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }

                            return new mpResponse(new binaryData("{\"good\":true, \"oneRepMax\":" + oneRepMax + "}"), 200);
                        }
                    )
                )
            );
            #endregion

            #region exercise-feedback
            root.addProperty("exercise-feedback",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            Console.Write("Exercise Feedback Data Requested...");

                            string requestData = req.data();
                            string requestEmail = "", requestPasswordEmailHash = "";
                            mpObject feedbackItem = null;

                            try {
                                mpObject requestJSON = (mpObject)mpJson.parse(requestData);

                                requestEmail = ((mpValue)requestJSON.getChild("email")).data.asString();
                                requestPasswordEmailHash = ((mpValue)requestJSON.getChild("passwordEmailHash")).data.asString();
                                feedbackItem = ((mpObject)requestJSON.getChild("feedbackItem"));
                                Plan plan = new Plan(db, manager.getUser(requestEmail, requestPasswordEmailHash));
                                string uuid = ((mpValue)feedbackItem.getChild("uuid")).data.asString();
                                workoutItem ex = plan.findItem(uuid);
                                ex.difficulty = ((mpValue)feedbackItem.getChild("difficulty")).data.asInt();
                                mpArray setInfo = ((mpArray)feedbackItem.getChild("sets"));
                                int user1RM = ((mpValue)((mpObject)((mpObject)feedbackItem.getChild("feedbackItem")).getChild("exercise")).getChild("user1RM")).data.asInt();
                                if (plan.exercise1rms.ContainsKey(uuid)) {
                                    plan.exercise1rms[uuid] = user1RM;
                                } else {
                                    plan.exercise1rms.Add(uuid, user1RM);
                                }
                                for (int i = 0; i < setInfo.allChildren.Count; i++) {
                                    try {
                                        ex.sets[i].repsCompleted = ((mpValue)((mpObject)setInfo[i]).getChild("repsCompleted")).data.asInt();
                                        ex.sets[i].doneWithRest = true;
                                    } catch (Exception e) { };

                                }
                            } catch (Exception ex) {
                                Console.WriteLine("Exercise Request Error: " + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }
                            return mpResponse.empty200();
                        }
                    )
                )
            );
            #endregion

            Console.ReadKey();

            server.stop();



        }
    }
}
