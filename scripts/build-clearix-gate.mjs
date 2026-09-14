// CLEARIX 페이지를 비밀번호로 보호되는 정적 페이지로 빌드합니다.
//
//   원본(비공개, git 제외):  _private/clearix/clearix.html + _private/clearix/images/*
//   출력(커밋·배포):         projects/clearix.html  — 본문 전체(이미지 포함)가 AES-256-GCM으로 암호화되어 들어가고,
//                            브라우저가 입력한 비밀번호로 키를 유도(PBKDF2-SHA256)해 복호화합니다.
//
// 사용법 (저장소 루트에서):
//   set CLEARIX_PAGE_PASSWORD=원하는비밀번호 && node scripts/build-clearix-gate.mjs        (cmd)
//   $env:CLEARIX_PAGE_PASSWORD='원하는비밀번호'; node scripts/build-clearix-gate.mjs      (PowerShell)
//   node scripts/build-clearix-gate.mjs --password 원하는비밀번호
//
// 비밀번호를 바꾸거나 원본을 수정했으면 다시 빌드해서 projects/clearix.html을 커밋하면 됩니다.
import { readFileSync, writeFileSync, existsSync } from 'node:fs';
import { randomBytes, pbkdf2Sync, createCipheriv } from 'node:crypto';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const SRC_HTML = path.join(ROOT, '_private', 'clearix', 'clearix.html');
const SRC_IMAGES = path.join(ROOT, '_private', 'clearix', 'images');
const TEMPLATE = path.join(ROOT, 'scripts', 'clearix-gate.template.html');
const OUT = path.join(ROOT, 'projects', 'clearix.html');
const PUBLIC_IMAGES = new Set(['clearix-title.jpg', 'clearix-cover.svg']); // 공개용 타이틀만 예외
const ITERATIONS = 310000;

const argIdx = process.argv.indexOf('--password');
const password = argIdx >= 0 ? process.argv[argIdx + 1] : process.env.CLEARIX_PAGE_PASSWORD;
if (!password || password.length < 4) {
  console.error('비밀번호가 필요합니다: CLEARIX_PAGE_PASSWORD 환경변수 또는 --password <값> (4자 이상)');
  process.exit(1);
}
if (!existsSync(SRC_HTML)) {
  console.error(`원본이 없습니다: ${SRC_HTML}`);
  process.exit(1);
}

const MIME = { '.png': 'image/png', '.jpg': 'image/jpeg', '.jpeg': 'image/jpeg', '.webp': 'image/webp', '.svg': 'image/svg+xml' };
let html = readFileSync(SRC_HTML, 'utf8');
let inlined = 0, bytes = 0;
html = html.replace(/src="\.\.\/images\/clearix\/([^"]+)"/g, (whole, name) => {
  if (PUBLIC_IMAGES.has(name)) return whole;
  const file = path.join(SRC_IMAGES, name);
  if (!existsSync(file)) throw new Error(`비공개 이미지가 없습니다: ${name}`);
  const buf = readFileSync(file);
  inlined += 1; bytes += buf.length;
  return `src="data:${MIME[path.extname(name).toLowerCase()] || 'application/octet-stream'};base64,${buf.toString('base64')}"`;
});

const salt = randomBytes(16);
const iv = randomBytes(12);
const key = pbkdf2Sync(password, salt, ITERATIONS, 32, 'sha256');
const cipher = createCipheriv('aes-256-gcm', key, iv);
const ct = Buffer.concat([cipher.update(Buffer.from(html, 'utf8')), cipher.final(), cipher.getAuthTag()]);

const template = readFileSync(TEMPLATE, 'utf8');
const out = template
  .replace('__SALT__', salt.toString('base64'))
  .replace('__IV__', iv.toString('base64'))
  .replace('__ITER__', String(ITERATIONS))
  .replace('__CIPHER__', ct.toString('base64'));
writeFileSync(OUT, out.replace(/\r?\n/g, '\r\n'), 'utf8');

console.log(`빌드 완료: ${path.relative(ROOT, OUT)}`);
console.log(`  원본 ${(Buffer.byteLength(html) / 1024).toFixed(0)} KB (이미지 ${inlined}개, ${(bytes / 1024).toFixed(0)} KB 인라인) → 출력 ${(Buffer.byteLength(out) / 1024).toFixed(0)} KB`);
