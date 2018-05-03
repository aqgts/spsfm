import path from "path";
import {fromByteArray} from "base64-js";
import mime from "mime";

export default {
  readBinaryFromFileAsync(inputFile) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        resolve(new Uint8Array(reader.result));
      };
      reader.onerror = reject;
      reader.readAsArrayBuffer(inputFile);
    });
  },
  toDataURLFromBinary(binary, extname = "") {
    return `data:${this.getMIMEType(extname)};base64,${fromByteArray(binary)}`;
  },
  getMIMEType(fileName) {
    const extname = fileName.startsWith(".") ? fileName : path.extname(fileName);
    let mimeType = mime.getType(extname.replace(/^\./, ""));
    if (mimeType === null) mimeType = "application/octet-stream";
    return mimeType;
  },
};
