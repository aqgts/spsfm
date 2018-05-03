import BezierCurve from "./bezier-curve";
import Vector2 from "./vector2";
import MyMath from "./my-math";

export default class FunctionLikeBezierCurve extends BezierCurve {
  constructor(controlPoint1, controlPoint2) {
    super(new Vector2(0, 0), controlPoint1, controlPoint2, new Vector2(1, 1));
  }
  toFunction() {
    const tToX = t => this.point(t).x;
    const tToY = t => this.point(t).y;
    const xToT = MyMath.inverse(tToX, 0, 1);
    return x => tToY(xToT(x));
  }
}
FunctionLikeBezierCurve.linear = new FunctionLikeBezierCurve(
  new Vector2(20 / 127, 20 / 127),
  new Vector2(107 / 127, 107 / 127)
);
