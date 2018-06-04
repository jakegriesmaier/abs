using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using HashLib;

namespace abs {
    public static class Util {
        public static int percent1RM(int percent, int weight) {
            return (int)(Math.Ceiling((weight * (percent / 100.0)) / 5) * 5);
        }

        public static T randomElement<T>(this HashSet<T> set) {
            if (set.Count == 0) throw new Exception("randomElement can't take an empty HashSet<...>");
            return set.ToArray()[rand(set.Count)];
        }

        public static SHA256Managed hasher = null;
        public static string hash(string val) {
            //if(hasher == null) hasher = HashFactory.Crypto.SHA3.CreateKeccak256();
            if (hasher == null) hasher = new SHA256Managed();
            
            byte[] rawBytes = Encoding.UTF8.GetBytes(val);
            byte[] hashedBytes = hasher.ComputeHash(rawBytes);
            byte[] base64Bytes = hashedBytes;
            String plainResult = Convert.ToBase64String(base64Bytes);


            return plainResult;
        }

        public static Random r;
        public static int rand(int max) {
            if (r == null) r = new Random();
            return r.Next(max);
        }
        public static string randomHash() {
            if (r == null) r = new Random();
            return hash(r.Next().ToString());
        }
    }
}
