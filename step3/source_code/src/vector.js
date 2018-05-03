export default class Vector {
  constructor(...values) {
    values.forEach((value, i) => {
      this[i] = value;
    });
    this.length = values.length;
  }
  clone() {
    return new this.constructor(...this.toArray());
  }
  negate() {
    return new this.constructor(...new Array(this.length).fill().map((_, i) => -this[i]));
  }
  add(other) {
    return new this.constructor(...new Array(this.length).fill().map((_, i) => this[i] + other[i]));
  }
  subtract(other) {
    return new this.constructor(...new Array(this.length).fill().map((_, i) => this[i] - other[i]));
  }
  multiply(scale) {
    return new this.constructor(...new Array(this.length).fill().map((_, i) => this[i] * scale));
  }
  divide(scale) {
    return new this.constructor(...new Array(this.length).fill().map((_, i) => this[i] / scale));
  }
  norm() {
    return Math.sqrt(this.squaredNorm());
  }
  squaredNorm() {
    return this.innerProduct(this);
  }
  innerProduct(other) {
    return new Array(this.length).fill().reduce((sum, _, i) => sum + this[i] * other[i], 0);
  }
  normalize() {
    return this.divide(this.norm());
  }
  equals(other) {
    return this === other || (this.length === other.length && new Array(this.length).fill().every((_, i) => this[i] === other[i]));
  }
  toString() {
    return `(${this.toArray().join(", ")})`;
  }
  toArray() {
    return Array.from(this);
  }
  static lerp(v1, v2, t) {
    return v1.add(v2.subtract(v1).multiply(t));
  }
  static zero(n) {
    return new this(...new Array(n).fill(0));
  }
}
