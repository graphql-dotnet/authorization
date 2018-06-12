import { exec, pushd, popd } from 'shelljs'
import Deferred from 'simple-make/lib/Deferred'
import settings from './settings'

export default function compile() {
  const deferred = new Deferred();

  const platform = process.platform === 'darwin'
    ? '-f netcoreapp2.0'
    : ''
  const build = `dotnet build ${platform} -c ${settings.target}`

  pushd('src/GraphQL.Authorization.Tests')
  console.log(build)

  exec(build, (code, stdout, stderr)=> {
    if(code === 0) {
      deferred.resolve()
    } else {
      deferred.reject(stderr)
    }
  });

  popd()

  return deferred.promise
}
