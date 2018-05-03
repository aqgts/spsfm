import Vector from "./vector";

export default class Vector2 extends Vector {
  constructor(x, y) {
    super(x, y);
  }
  get x() {
    return this[0];
  }
  get y() {
    return this[1];
  }
}
Vector2.zero = new Vector2(0, 0);