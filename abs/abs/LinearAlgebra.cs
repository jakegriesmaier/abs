using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace abs {
    struct Vec2 {
        public double x, y;
        
        public static Vec2 operator -(Vec2 lhs) {
            return new Vec2(-lhs.x, -lhs.y);
        }
        public static Vec2 operator +(Vec2 lhs, Vec2 rhs) {
            return new Vec2(lhs.x + rhs.x, lhs.y + rhs.y);
        }
        public static Vec2 operator -(Vec2 lhs, Vec2 rhs) {
            return new Vec2(lhs.x - rhs.x, lhs.y - rhs.y);
        }
        public static Vec2 operator *(Vec2 lhs, double mul) {
            return new Vec2(lhs.x * mul, lhs.y * mul);
        }
        public static Vec2 operator /(Vec2 lhs, double div) {
            return new Vec2(lhs.x / div, lhs.y / div);
        }

        public static double dot(Vec2 lhs, Vec2 rhs) {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        public double norm() {
            return Math.Sqrt(x * x + y * y);
        }
        public double norm2() {
            return x * x + y * y;
        }
        public Vec2 normalized() {
            return this / norm();
        }

        public Vec2(double v) {
            this.x = v;
            this.y = v;
        }
        public Vec2(double x, double y) {
            this.x = x;
            this.y = y;
        }
    }
}
