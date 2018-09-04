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

            db.deleteTableIfExists("usercalibration");
            db.createTableIfNeeded("usercalibration", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("email", "text"),
                new KeyValuePair<string, string>("exercise", "text"),
                new KeyValuePair<string, string>("reps", "int"),
                new KeyValuePair<string, string>("weight", "int"),
                new KeyValuePair<string, string>("recorded", "date")
            }));

            db.deleteTableIfExists("workoutdays");
            db.createTableIfNeeded("workoutdays", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("uuid", "text"),
                new KeyValuePair<string, string>("associateduser", "text"),
                new KeyValuePair<string, string>("workoutdate", "date"),
                new KeyValuePair<string, string>("primarygroup", "text"),
                new KeyValuePair<string, string>("secondarygroup", "text"),
                new KeyValuePair<string, string>("itemcount", "int") //number of exercises
            }));

            db.deleteTableIfExists("workoutitems");
            db.createTableIfNeeded("workoutitems", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("uuid", "text"),
                new KeyValuePair<string, string>("associatedday", "text"), //uuid of associated day
                new KeyValuePair<string, string>("exercisename", "text"),
                new KeyValuePair<string, string>("setcount", "int"),
                new KeyValuePair<string, string>("difficulty", "int"),
                new KeyValuePair<string, string>("onerepmax", "real")
            }));

            db.deleteTableIfExists("workoutsets");
            db.createTableIfNeeded("workoutsets", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("uuid", "text"),
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
                                UserDataAccess info = new UserDataAccess(db, user);
                                WorkoutGenerator p = new WorkoutGenerator(info);

                                mpResponse res = mpResponse.success();
                                var day = p.generateDay(requestNumItems);
                                res.response = new binaryData(day.toJSON(info).ToString());
                                info.AddDay(day);
                                info.Store();
                                info.Dispose();

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

                                UserDataAccess info = new UserDataAccess(db, manager.getUser(requestEmail, requestPasswordEmailHash));
                                info.IngestCalibrationInfo(
                                    exName,
                                    ((mpValue)requestJSON.getChild("reps")).data.asInt(),
                                    ((mpValue)requestJSON.getChild("weight")).data.asInt()
                                );
                                info.Store();
                                oneRepMax = info.GetMostRecentCalibratedOneRepMax(exName).Value;
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

                            try {
                                mpObject requestJSON = (mpObject)mpJson.parse(requestData);

                                requestEmail = ((mpValue)requestJSON.getChild("email")).data.asString();
                                requestPasswordEmailHash = ((mpValue)requestJSON.getChild("passwordEmailHash")).data.asString();

                                mpObject feedback = (mpObject)requestJSON.getChild("feedback");

                                WorkoutItem item = new WorkoutItem(feedback);

                                UserDataAccess access = new UserDataAccess(db, manager.getUser(requestEmail, requestPasswordEmailHash));
                                
                                access.UpdateItem(item);
                                access.Store();
                                access.Dispose();

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

            Exercise.getAllExercises(db);

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
