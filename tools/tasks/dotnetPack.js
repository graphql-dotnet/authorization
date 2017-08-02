import { exec } from 'shelljs';
import Deferred from 'simple-make/lib/Deferred';
import settings from './settings';

export default function compile() {
  const deferred = new Deferred();

  let versionSuffix = ''

  if(settings.versionSuffix.length > 0) {
    versionSuffix = `--version-suffix ${settings.versionSuffix}${settings.revision}`;
  }

  const cmd = `dotnet pack src/GraphQL.Authorization -o ${settings.artifacts} -c ${settings.target} /p:PackageVersion=${settings.version} ${versionSuffix}`
  console.log(cmd);

  exec(cmd, (code, stdout, stderr)=> {
    if(code === 0) {
      deferred.resolve();
    } else {
      deferred.reject(stderr);
    }
  });
  return deferred.promise;
}
