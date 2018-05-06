import VMD from "./vmd";
import Vector3 from "./vector3";
import Quaternion from "./quaternion";
import FunctionLikeBezierCurve from "./function-like-bezier-curve";

export default {
  async runAsync(x, y, z, rx, ry, rz) {
    const unitDegree = 60;
    const unitRadian = unitDegree * Math.PI / 180;
    const angleOfView = Math.round((function bisection(min, max) {
      const mid = (min + max) / 2;
      if (mid === min || mid === max) return mid;
      const value = Math.sin(mid - unitRadian) * Math.cos(mid / 2) - 0.3 * Math.cos(mid / 2 - unitRadian) * Math.sin(mid);
      if (value < 0) return bisection(mid, max);
      if (value > 0) return bisection(min, mid);
      if (value === 0) return mid;
    })(0, Math.PI) / Math.PI * 180) / 180 * Math.PI;
    const angles = [
      Quaternion.yxzEuler(Math.PI / 3, -Math.PI / 3, 0),
      Quaternion.yxzEuler(Math.PI, -Math.PI / 3, 0),
      Quaternion.yxzEuler(-Math.PI / 3, -Math.PI / 3, 0),
      Quaternion.yxzEuler(Math.PI / 3, Math.PI / 3, 0),
      Quaternion.yxzEuler(Math.PI, Math.PI / 3, 0),
      Quaternion.yxzEuler(-Math.PI / 3, Math.PI / 3, 0),
      Quaternion.yxzEuler(0, 0, 0),
      Quaternion.yxzEuler(Math.PI / 3, 0, 0),
      Quaternion.yxzEuler(Math.PI * 2 / 3, 0, 0),
      Quaternion.yxzEuler(Math.PI, 0, 0),
      Quaternion.yxzEuler(-Math.PI * 2 / 3, 0, 0),
      Quaternion.yxzEuler(-Math.PI / 3, 0, 0),
    ];
    const baseRotation = Quaternion.yxzEuler(-ry, rx, rz);

    const motion = new VMD(VMD.CAMERA_MODEL_NAME, {
      bone: [],
      morph: [],
      camera: angles.map((angle, frameNumber) => new VMD.CameraKeyFrame(
        frameNumber,
        0,
        new Vector3(x, y, z),
        baseRotation.multiply(angle),
        {
          x: FunctionLikeBezierCurve.linear,
          y: FunctionLikeBezierCurve.linear,
          z: FunctionLikeBezierCurve.linear,
          rotation: FunctionLikeBezierCurve.linear,
          distance: FunctionLikeBezierCurve.linear,
          angleOfView: FunctionLikeBezierCurve.linear
        },
        angleOfView,
        true
      )),
      light: [],
      selfShadow: [],
      showIK: []
    });
    return motion.write();
  }
};
