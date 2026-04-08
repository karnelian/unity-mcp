import { cpSync, existsSync, mkdirSync } from "fs";
import { resolve, dirname } from "path";
import { fileURLToPath } from "url";

const __dirname = dirname(fileURLToPath(import.meta.url));
const src = resolve(__dirname, "../../unity-plugin/Editor");
const dest = resolve(__dirname, "../unity-plugin/Editor");

if (existsSync(src)) {
  mkdirSync(dirname(dest), { recursive: true });
  cpSync(src, dest, { recursive: true, force: true });
  // Also copy unity-plugin/package.json
  const pkgSrc = resolve(__dirname, "../../unity-plugin/package.json");
  const pkgDest = resolve(__dirname, "../unity-plugin/package.json");
  if (existsSync(pkgSrc)) cpSync(pkgSrc, pkgDest, { force: true });
  console.log("✅ unity-plugin copied into server package");
} else {
  console.log("⏭️  unity-plugin source not found (already in package)");
}
