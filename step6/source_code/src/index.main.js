import "babel-polyfill";
import BinaryUtils from "./binary-utils";

const loadAsync = async (file) => {
  const binary = await BinaryUtils.readBinaryFromFileAsync(file);
  const url = BinaryUtils.toDataURLFromBinary(binary, file.name);

  while (document.body.childElementCount > 0) {
    document.body.removeChild(document.body.firstChild);
  }

  const scene = document.createElement("a-scene");
  const sky = document.createElement("a-sky");
  sky.setAttribute("src", url);
  scene.appendChild(sky);
  document.body.appendChild(scene);
};

document.addEventListener("DOMContentLoaded", () => {
  document.getElementsByTagName("input")[0].addEventListener("change", async event => {
    if (event.target.files.length === 0) return;
    loadAsync(event.target.files[0]);
  });
  document.documentElement.addEventListener("dragover", event => {
    event.preventDefault();
  }, false);
  document.documentElement.addEventListener("drop", function eventListener(event) {
    event.preventDefault();
    document.body.removeEventListener("drop", eventListener);
    loadAsync(event.dataTransfer.files[0]);
  }, false);
});
