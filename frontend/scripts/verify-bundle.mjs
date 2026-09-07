import { readdir, readFile } from 'node:fs/promises';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const bundleDirectory = new URL('../dist/frontend/browser/', import.meta.url);
const bundleDirectoryPath = fileURLToPath(bundleDirectory);
const fileNames = (await readdir(bundleDirectory)).filter((fileName) => fileName.endsWith('.js'));
const forbiddenPatterns = [
  /(?:localhost|127\.0\.0\.1|api):\d+/i,
  /JWT_[A-Z_]+/i,
  /INITIAL_USER_[A-Z_]+/i,
  /change-this-(?:postgres|signing)/i,
];

for (const fileName of fileNames) {
  const contents = await readFile(join(bundleDirectoryPath, fileName), 'utf8');
  const match = forbiddenPatterns.find((pattern) => pattern.test(contents));
  if (match) {
    throw new Error(`Bundle contém padrão proibido (${match}) em ${fileName}.`);
  }
}

console.log(`Verificação do bundle concluída: ${fileNames.length} arquivos JavaScript inspecionados.`);
