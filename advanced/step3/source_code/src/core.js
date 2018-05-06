import VMD from "../../../../step3/source_code/src/vmd";
import Vector3 from "../../../../step3/source_code/src/vector3";
import Quaternion from "../../../../step3/source_code/src/quaternion";
import FunctionLikeBezierCurve from "../../../../step3/source_code/src/function-like-bezier-curve";

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
      ["01.vmd", Quaternion.yxzEuler(Math.PI / 3, -Math.PI / 3, 0)],
      ["02.vmd", Quaternion.yxzEuler(Math.PI, -Math.PI / 3, 0)],
      ["03.vmd", Quaternion.yxzEuler(-Math.PI / 3, -Math.PI / 3, 0)],
      ["04.vmd", Quaternion.yxzEuler(Math.PI / 3, Math.PI / 3, 0)],
      ["05.vmd", Quaternion.yxzEuler(Math.PI, Math.PI / 3, 0)],
      ["06.vmd", Quaternion.yxzEuler(-Math.PI / 3, Math.PI / 3, 0)],
      ["07.vmd", Quaternion.yxzEuler(0, 0, 0)],
      ["08.vmd", Quaternion.yxzEuler(Math.PI / 3, 0, 0)],
      ["09.vmd", Quaternion.yxzEuler(Math.PI * 2 / 3, 0, 0)],
      ["10.vmd", Quaternion.yxzEuler(Math.PI, 0, 0)],
      ["11.vmd", Quaternion.yxzEuler(-Math.PI * 2 / 3, 0, 0)],
      ["12.vmd", Quaternion.yxzEuler(-Math.PI / 3, 0, 0)],
    ];
    const baseRotation = Quaternion.yxzEuler(-ry, rx, rz);

    const zip = new JSZip();
    for (const [fileName, angle] of angles) {
      const motion = new VMD(VMD.CAMERA_MODEL_NAME, {
        bone: [],
        morph: [],
        camera: [new VMD.CameraKeyFrame(
          0,
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
        )],
        light: [],
        selfShadow: [],
        showIK: []
      });
      zip.file(fileName, motion.write());
    }
    return await zip.generateAsync({type: "uint8array"});
  }
};
