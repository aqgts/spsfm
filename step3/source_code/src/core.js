import VMD from "./vmd";
import Vector3 from "./vector3";
import Quaternion from "./quaternion";
import FunctionLikeBezierCurve from "./function-like-bezier-curve";

export default {
  run(x, y, z, rx, ry, rz) {
    const unitDegree = 30;
    const unitRadian = unitDegree * Math.PI / 180;
    const yawCount = 360 / unitDegree;
    const pitchCount = 180 / unitDegree;
    const angleOfView = Math.round((function bisection(min, max) {
      const mid = (min + max) / 2;
      if (mid === min || mid === max) return mid;
      const value = Math.sin(mid - unitRadian) * Math.cos(mid / 2) - 0.3 * Math.cos(mid / 2 - unitRadian) * Math.sin(mid);
      if (value < 0) return bisection(mid, max);
      if (value > 0) return bisection(min, mid);
      if (value === 0) return mid;
    })(0, Math.PI) / Math.PI * 180) / 180 * Math.PI;
    const baseRotation = Quaternion.yxzEuler(-ry, rx, rz);
    const motion = new VMD(VMD.CAMERA_MODEL_NAME, {
      bone: [],
      morph: [],
      camera: new Array(yawCount * pitchCount).fill().map((_, frameNumber) => {
        const yaw = frameNumber % yawCount * unitRadian;
        const pitch = Math.floor(frameNumber / yawCount) * unitRadian - (Math.PI / 2 - unitRadian / 2);
        return new VMD.CameraKeyFrame(
          frameNumber,
          0,
          new Vector3(x, y, z),
          baseRotation.multiply(Quaternion.yxzEuler(yaw, pitch, 0)),
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
        );
      }),
      light: [],
      selfShadow: [],
      showIK: []
    });
    return motion.write();
  }
};
