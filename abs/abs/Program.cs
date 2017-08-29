using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using monopage;
using System.Globalization;

namespace abs {
    class Program {
        static void Main(string[] args) {
            Server server = new Server();
            mpBase mpBase = new mpBase();

            server.start(mpBase.restful, "http://*:8080/");
            mpBase.addProperty("test", "the value at test");

            mpPageAssembler assembler = new mpPageAssembler(
                new mpBetterFile("../../Web/Templates/Template.html"),
                new mpBetterFile("../../Web/Templates/Template.js"),
                new mpBetterFile("../../Web/Templates/Template.css"),
                new mpPageElement("../../Web/Header"),
                new mpPageElement("../../Web/Footer")
            );
            assembler.addContent("test", new mpPageElement("../../Web/Content"));
            assembler.addContent("test", new mpPageElement("../../Web/Content"));
            assembler.addContent("test", new mpPageElement("../../Web/Content"));
            assembler.addContent("test", new mpPageElement("../../Web/Content"));


            mpBase.addProperty("web", assembler);

            Console.ReadKey();

            server.stop();
        }
    }

    class mpBetterFile : mpReadonlyEndpoint {
        public bool OK { get; private set; }
        public readonly string location;
        private binaryData cache;
        public TimeSpan cacheTimeout;
        private DateTime lastLoaded;

        public override string tabbedJson(int tab, bool tabFirstLine) {
            return data.toJson();
        }
        public override mpObject meta {
            get {
                return new mpObject(
                    new mpProperty("type", cache.type.ToString()),
                    new mpProperty("OK", OK),
                    new mpProperty("lastLoaded", location == null ? DateTime.UtcNow.ToString(DateTimeFormatInfo.InvariantInfo) : lastLoaded.ToString(DateTimeFormatInfo.InvariantInfo))
                );
            }
        }
        public override binaryData data {
            get {
                if (location != null && DateTime.UtcNow - lastLoaded > cacheTimeout) {
                    load();
                }
                return cache;
            }
        }
        public override mpResponse response {
            get {
                if (OK) {
                    return new mpResponse(data, 200);
                } else {
                    return new mpResponse(500);
                }
            }
        }

        private void load() {
            if (location != null) {
                lastLoaded = DateTime.UtcNow;
                try {
                    cache = binaryData.fromFile(location);
                    OK = true;
                } catch {
                    cache = new binaryData("");
                    OK = false;
                }
            }
        }

        public mpBetterFile(binaryData data) {
            this.cache = data;
            this.location = null;
            this.lastLoaded = DateTime.UtcNow;
            this.cacheTimeout = TimeSpan.Zero;
            this.OK = true;
        }
        public mpBetterFile(string location) : this(location, new TimeSpan(0,0,5)) {
        }
        public mpBetterFile(string location, TimeSpan cacheTimeout) {
            this.location = location;
            this.cacheTimeout = cacheTimeout;
            load();
        }
    }
}
