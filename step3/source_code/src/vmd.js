import SequentialAccessBinary from "./sequential-access-binary";

export default class VMD {
  constructor(modelName, keyFrames) {
    this.modelName = modelName;
    this.keyFrames = keyFrames;
  }
  write() {
    const io = new SequentialAccessBinary();
    io.writeNullTerminatedString("Vocaloid Motion Data 0002", 30, "shift_jis");
    if (this.modelName === this.constructor.CAMERA_MODEL_NAME) {
      io.writeString(this.constructor.CAMERA_MODEL_NAME, "shift_jis");
    } else {
      io.writeNullTerminatedString(this.modelName, 20, "shift_jis");
    }
    io.writeUint32([...this.keyFrames.bone.values()].map(keyFrames => keyFrames.length).reduce((sum, x) => sum + x, 0));
    this.keyFrames.bone.forEach(keyFrames => {
      keyFrames.forEach(keyFrame => {
        keyFrame.write(io);
      });
    });
    io.writeUint32(this.keyFrames.morph.length);
    this.keyFrames.morph.forEach(keyFrame => {
      keyFrame.write(io);
    });
    io.writeUint32(this.keyFrames.camera.length);
    this.keyFrames.camera.forEach(keyFrame => {
      keyFrame.write(io);
    });
    io.writeUint32(this.keyFrames.light.length);
    this.keyFrames.light.forEach(keyFrame => {
      keyFrame.write(io);
    });
    io.writeUint32(this.keyFrames.selfShadow.length);
    this.keyFrames.selfShadow.forEach(keyFrame => {
      keyFrame.write(io);
    });
    io.writeUint32(this.keyFrames.showIK.length);
    this.keyFrames.showIK.forEach(keyFrame => {
      keyFrame.write(io);
    });
    return io.toUint8Array();
  }
}
VMD.CAMERA_MODEL_NAME = "カメラ・照明\0on Data";
VMD.BoneKeyFrame = class BoneKeyFrame {
  constructor(boneName, frameNumber, position, quaternion, bezierCurves) {
    this.boneName = boneName;
    this.frameNumber = frameNumber;
    this.position = position;
    this.quaternion = quaternion;
    this.bezierCurves = bezierCurves;
  }
  write(io) {
    io.writeNullTerminatedString(this.boneName, 15, "shift_jis");
    io.writeUint32(this.frameNumber);
    io.writeFloat32Array(Array.from(this.position));
    io.writeFloat32Array(Array.from(this.quaternion.toVector()));
    io.writeInt8Array([
      this.bezierCurves.x.controlPoints[1].x, this.bezierCurves.y.controlPoints[1].x,
      this.bezierCurves.z.controlPoints[1].x, this.bezierCurves.rotation.controlPoints[1].x,
      this.bezierCurves.x.controlPoints[1].y, this.bezierCurves.y.controlPoints[1].y,
      this.bezierCurves.z.controlPoints[1].y, this.bezierCurves.rotation.controlPoints[1].y,
      this.bezierCurves.x.controlPoints[2].x, this.bezierCurves.y.controlPoints[2].x,
      this.bezierCurves.z.controlPoints[2].x, this.bezierCurves.rotation.controlPoints[2].x,
      this.bezierCurves.x.controlPoints[2].y, this.bezierCurves.y.controlPoints[2].y,
      this.bezierCurves.z.controlPoints[2].y, this.bezierCurves.rotation.controlPoints[2].y,
                                              this.bezierCurves.y.controlPoints[1].x,
      this.bezierCurves.z.controlPoints[1].x, this.bezierCurves.rotation.controlPoints[1].x,
      this.bezierCurves.x.controlPoints[1].y, this.bezierCurves.y.controlPoints[1].y,
      this.bezierCurves.z.controlPoints[1].y, this.bezierCurves.rotation.controlPoints[1].y,
      this.bezierCurves.x.controlPoints[2].x, this.bezierCurves.y.controlPoints[2].x,
      this.bezierCurves.z.controlPoints[2].x, this.bezierCurves.rotation.controlPoints[2].x,
      this.bezierCurves.x.controlPoints[2].y, this.bezierCurves.y.controlPoints[2].y,
      this.bezierCurves.z.controlPoints[2].y, this.bezierCurves.rotation.controlPoints[2].y,
      0,
      this.bezierCurves.z.controlPoints[1].x, this.bezierCurves.rotation.controlPoints[1].x,
      this.bezierCurves.x.controlPoints[1].y, this.bezierCurves.y.controlPoints[1].y,
      this.bezierCurves.z.controlPoints[1].y, this.bezierCurves.rotation.controlPoints[1].y,
      this.bezierCurves.x.controlPoints[2].x, this.bezierCurves.y.controlPoints[2].x,
      this.bezierCurves.z.controlPoints[2].x, this.bezierCurves.rotation.controlPoints[2].x,
      this.bezierCurves.x.controlPoints[2].y, this.bezierCurves.y.controlPoints[2].y,
      this.bezierCurves.z.controlPoints[2].y, this.bezierCurves.rotation.controlPoints[2].y,
      0, 0,
                                              this.bezierCurves.rotation.controlPoints[1].x,
      this.bezierCurves.x.controlPoints[1].y, this.bezierCurves.y.controlPoints[1].y,
      this.bezierCurves.z.controlPoints[1].y, this.bezierCurves.rotation.controlPoints[1].y,
      this.bezierCurves.x.controlPoints[2].x, this.bezierCurves.y.controlPoints[2].x,
      this.bezierCurves.z.controlPoints[2].x, this.bezierCurves.rotation.controlPoints[2].x,
      this.bezierCurves.x.controlPoints[2].y, this.bezierCurves.y.controlPoints[2].y,
      this.bezierCurves.z.controlPoints[2].y, this.bezierCurves.rotation.controlPoints[2].y,
      0, 0, 0
    ].map(v => Math.round(v * 127)));
  }
};
VMD.CameraKeyFrame = class CameraKeyFrame {
  constructor(frameNumber, distance, position, quaternion, bezierCurves, angleOfView, isPerspectiveMode) {
    this.frameNumber = frameNumber;
    this.distance = distance;
    this.position = position;
    this.quaternion = quaternion;
    this.bezierCurves = bezierCurves;
    this.angleOfView = angleOfView;
    this.isPerspectiveMode = isPerspectiveMode;
  }
  write(io) {
    const [yaw, pitch, roll] = this.quaternion.yxzEulerAngles();
    io.writeUint32(this.frameNumber);
    io.writeFloat32(-this.distance);
    io.writeFloat32Array(Array.from(this.position));
    io.writeFloat32Array([-pitch, yaw, roll]);
    io.writeInt8Array([
      this.bezierCurves.x.controlPoints[1].x, this.bezierCurves.x.controlPoints[2].x,
      this.bezierCurves.x.controlPoints[1].y, this.bezierCurves.x.controlPoints[2].y,
      this.bezierCurves.y.controlPoints[1].x, this.bezierCurves.y.controlPoints[2].x,
      this.bezierCurves.y.controlPoints[1].y, this.bezierCurves.y.controlPoints[2].y,
      this.bezierCurves.z.controlPoints[1].x, this.bezierCurves.z.controlPoints[2].x,
      this.bezierCurves.z.controlPoints[1].y, this.bezierCurves.z.controlPoints[2].y,
      this.bezierCurves.rotation.controlPoints[1].x, this.bezierCurves.rotation.controlPoints[2].x,
      this.bezierCurves.rotation.controlPoints[1].y, this.bezierCurves.rotation.controlPoints[2].y,
      this.bezierCurves.distance.controlPoints[1].x, this.bezierCurves.distance.controlPoints[2].x,
      this.bezierCurves.distance.controlPoints[1].y, this.bezierCurves.distance.controlPoints[2].y,
      this.bezierCurves.angleOfView.controlPoints[1].x, this.bezierCurves.angleOfView.controlPoints[2].x,
      this.bezierCurves.angleOfView.controlPoints[1].y, this.bezierCurves.angleOfView.controlPoints[2].y
    ].map(v => Math.round(v * 127)));
    io.writeUint32(Math.round(this.angleOfView * 180 / Math.PI));
    io.writeInt8(this.isPerspectiveMode ? 0 : 1);
  }
};
