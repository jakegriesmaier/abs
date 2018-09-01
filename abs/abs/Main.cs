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

            //db.query("INSERT INTO workoutdays VALUES('1234', 'bob@bob.com', '2018-01-05', 'chest', 'upperchest', 1);");
            //db.query("INSERT INTO workoutitems VALUES('5678', '1234', 'Barbell Bench Press ', 1, 0);");
            //db.query("INSERT INTO workoutsets VALUES('5678', 10, 75, 30, 9);");
        }

        /// <summary>
        /// manages user creating accounts
        /// </summary>
        static void SetupRegistrationManager(mpBase root, Database db, UserManager manager) {
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
        }

        /// <summary>
        /// informs user if their login credentials are valid or not
        /// </summary>
        static void SetupLoginManager(mpBase root, Database db, UserManager manager) {
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
        }

        /// <summary>
        /// manages creating exercises and responding to the user with information about that request
        /// including all the exercises that were created during the request
        /// </summary>
        /// <param name="root"></param>
        /// <param name="db"></param>
        /// <param name="manager"></param>
        static void SetupExerciseInfoManager(mpBase root, Database db, UserManager manager) {
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
                                Plan p = new Plan(db, user);

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
        }

        /// <summary>
        /// takes in calibration information for a certain exercise and returns the calculated 1rm
        /// for that exercise
        /// </summary>
        /// <param name="root"></param>
        /// <param name="db"></param>
        /// <param name="manager"></param>
        static void SetupExerciseCalibrationManager(mpBase root, Database db, UserManager manager) {
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
                            double oneRepMax = -1;

                            try {
                                mpObject requestJSON = (mpObject)mpJson.parse(requestData);

                                requestEmail = ((mpValue)requestJSON.getChild("email")).data.asString();
                                requestPasswordEmailHash = ((mpValue)requestJSON.getChild("passwordEmailHash")).data.asString();
                                string exName = ((mpValue)requestJSON.getChild("exercise")).data.asString();

                                UserInfo info = new UserInfo(db, manager.getUser(requestEmail, requestPasswordEmailHash));
                                info.IngestCalibrationInfo(
                                    exName,
                                    ((mpValue)requestJSON.getChild("reps")).data.asInt(),
                                    ((mpValue)requestJSON.getChild("weight")).data.asInt()
                                );
                                info.Store();
                                oneRepMax = info.GetOneRepMax(exName).value;
                            } catch (Exception ex) {
                                Console.WriteLine("Calibration Info Error: " + ex.Message);
                                return new mpResponse(new binaryData("{\"good\":false, \"message\":\"" + ex.Message + "\"}"), 400);
                            }

                            return new mpResponse(new binaryData("{\"good\":true, \"oneRepMax\":" + oneRepMax + "}"), 200);
                        }
                    )
                )
            );
        }

        /// <summary>
        /// takes in feedback for a workout item and passes it to the user's plan for processing
        /// </summary>
        /// <param name="root"></param>
        /// <param name="db"></param>
        /// <param name="manager"></param>
        static void SetupExerciseFeedbackManager(mpBase root, Database db, UserManager manager) {
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
                            return new mpResponse(new binaryData("{\"good\":true}"), 200);
                        }
                    )
                )
            );
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
            
            mpBase root = new mpBase();

            mpServer server = new mpServer();
            server.start(root.restful, "http://*:8080/");

            SetupRegistrationManager(root, db, manager);
            SetupLoginManager(root, db, manager);
            SetupExerciseInfoManager(root, db, manager);
            SetupExerciseCalibrationManager(root, db, manager);
            SetupExerciseFeedbackManager(root, db, manager);

            Console.ReadKey();

            server.stop();



        }
    }
}
