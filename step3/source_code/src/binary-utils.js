import path from "path";
import mime from "mime";

export default {
  saveBinaryAsFile(binary, fileName, mimeType = this.getMIMEType(fileName)) {
    const blob = new Blob([binary], {type: mimeType});
    if (navigator.msSaveBlob) {
      navigator.msSaveBlob(blob, fileName);
    } else {
      const a = document.createElement("a");
      document.body.appendChild(a);
      a.setAttribute("href", URL.createObjectURL(blob));
      a.setAttribute("download", fileName);
      a.click();
      document.body.removeChild(a);
    }
  },
  getMIMEType(fileName) {
    const extname = fileName.startsWith(".") ? fileName : path.extname(fileName);
    let mimeType = mime.getType(extname.replace(/^\./, ""));
    if (mimeType === null) mimeType = "application/octet-stream";
    return mimeType;
  },
};
