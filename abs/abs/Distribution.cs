using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace abs {
    class Distribution<T> {
        private Dictionary<T, double> weights = new Dictionary<T, double>();
        private double total = 0;

        public void setWeight(T val, double weight) {
            if (weight < 0) throw new Exception("Weight cannot be negative");

            if (weight == 0) {
                weights.Remove(val);
            } else {
                weights[val] = weight;
            }

            total = weights.Select(kvp => kvp.Value).Sum();
        }
        public double this[T index] {
            get { return weights.ContainsKey(index) ? weights[index] : 0.0; }
            set { setWeight(index, value); }
        }

        public T select() {
            double v = Util.rand(0.0, total);
            double current = 0.0;

            KeyValuePair<T, double> kvp = new KeyValuePair<T, double>(default(T), 0.0);
            var it = weights.GetEnumerator();
            while (it.MoveNext()) {
                kvp = it.Current;
                current += kvp.Value;
                if (v <= current) {
                    return kvp.Key;
                }
            }
            return kvp.Key;
        }
        public List<T> selectN(int n) {
            List<T> val = new List<T>();
            for (int i = 0; i < n; i++) {
                val.Add(select());
            }
            return val;
        }
        public List<T> selectN_unique(int n) {
            if (n > weights.Count) throw new Exception("There are not enough elements in the set");
            else if (n == weights.Count) return weights.Select(kvp => kvp.Key).ToList();

            Dictionary<T, double> copy = new Dictionary<T, double>(weights);

            List<T> res = new List<T>();
            for(int i = 0; i < n; i++) {
                T selected = select();
                res.Add(selected);
                weights.Remove(selected);
            }

            weights = copy;
            return res;
        }
    }
}
