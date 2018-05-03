import MyMath from "./my-math";

export default class BezierCurve {
  constructor(...controlPoints) {
    if (controlPoints.length == 0) throw new Error("Empty control points.");
    this.controlPoints = controlPoints;
  }
  clone() {
    return new this.constructor(...this.controlPoints.map(controlPoint => controlPoint.clone()));
  }
  degree() {
    return this.controlPoints.length - 1;
  }
  point(t) {
    return this.controlPoints
      .map((controlPoint, i) => controlPoint.multiply(MyMath.binom(this.degree(), i) * Math.pow(t, i) * Math.pow(1 - t, this.degree() - i)))
      .reduce((sum, v) => sum.add(v));
  }
}
