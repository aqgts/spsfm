import "babel-polyfill";
import "./post-cdn";
import Core from "./core";
import TextAreaWrapper from "./text-area-wrapper";
import BinaryUtils from "./binary-utils";

let log = null;
new Vue({
  el: ".container",
  data: {
    x: "0",
    y: "0",
    z: "0",
    rx: "0",
    ry: "0",
    rz: "0",
    unitDegree: "30",
    isLoading: true,
    isProcessing: false
  },
  computed: {
  },
  methods: {
    async createMotion() {
      this.isProcessing = true;
      try {
        log.clear();
        const binary = Core.run(Number(this.x), Number(this.y), Number(this.z), Number(this.rx) / 180 * Math.PI, Number(this.ry) / 180 * Math.PI, Number(this.rz) / 180 * Math.PI);
        BinaryUtils.saveBinaryAsFile(binary, "camera.vmd");
        await log.appendAsync("モーションの作成に成功しました");
      } catch (error) {
        await log.appendAsync(`[Error]${error}`);
        throw error;
      } finally {
        this.isProcessing = false;
      }
    }
  },
  watch: {
  },
  mounted() {
    log = new TextAreaWrapper(document.getElementById("log"));
    this.isLoading = false;
  }
});
