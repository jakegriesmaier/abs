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
                new KeyValuePair<string, string>("username", "text"),
                new KeyValuePair<string, string>("email", "text"),
                new KeyValuePair<string, string>("salt", "text"),
                new KeyValuePair<string, string>("password", "text")
            }));

            db.deleteTableIfExists("userinfo");
            db.createTableIfNeeded("userinfo", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("associateduser", "text"),
                new KeyValuePair<string, string>("birthday", "text"),
                new KeyValuePair<string, string>("gender", "char(1)"),
                new KeyValuePair<string, string>("onerepmax", "int"),
                new KeyValuePair<string, string>("experience", "int"),
                new KeyValuePair<string, string>("exerciseinfo","text")
            }));

            db.deleteTableIfExists("plans");
            db.createTableIfNeeded("plans", new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("associateduser", "text"),
                new KeyValuePair<string, string>("workouttimes", "text"),
                new KeyValuePair<string, string>("equipmentavailable", "text"),
                new KeyValuePair<string, string>("goal", "char(1)")
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

            db.query("INSERT INTO workoutdays VALUES('1234', 'Bob', '2018-01-05', 'chest', 'upperchest', 1);");
            db.query("INSERT INTO workoutitems VALUES('5678', '1234', 'Barbell Bench Press ', 1, 0);");
            db.query("INSERT INTO workoutsets VALUES('5678', 10, 75, 30, 9);");
            db.query("INSERT INTO plans VALUES('Bob', '1,2,3,4,5,6,7', '', 'm');"); //bobs workout plan
            db.query("INSERT INTO users VALUES('Bob', 'bob@bob.com', 'seasoning', '36d85e');"); //bobs 
        }


        static void Main(string[] args) {
            Database db = new Database("Server=localhost;Port=5432;Username=postgres;Password=root;Database=postgres");
            ResetDatabase(db);

            HashSet<Exercise> exercises = Exercise.getAllExercises(db);

            Plan p = new Plan(db, new User("Bob"));

            WorkoutDay day = p.generateDay(3);

            String str = day.toJSON().ToString();
            File.WriteAllText("D:\\Desktop\\jsonTest.txt", str);

            mpBase root = new mpBase();

            mpServer server = new mpServer();
            server.start(root.restful, "http://*:8080/");

            root.addProperty("register",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            Console.Write("Got Register Request... ");
                            Thread.Sleep(1000);
                            mpResponse res = mpResponse.success();
                            res.response = new binaryData("[\"abc\"]");
                            Console.WriteLine("Returning");
                            return res;
                        }
                    )
                )
            );

            root.addProperty("login",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            mpResponse res = mpResponse.success();
                            res.response = new binaryData("LOGIN POST RESPONSE");
                            return res;
                        }
                    )
                )
            );

            root.addProperty("exercise-info",
                new mpRestfulTarget(
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            return mpResponse.empty400();
                        }
                    ),
                    new Func<System.Net.HttpListenerRequest, mpResponse>(
                        req => {
                            Console.Write("Requested...");
                            Thread.Sleep(2000);
                            Console.WriteLine("Responded!");
                            mpResponse res = mpResponse.success();
                            res.response = new binaryData(str);
                            return res;
                        }
                    )
                )
            );


            Console.ReadKey();

            server.stop();



        }
    }
}
