import { exec, rm } from 'shelljs'
import Deferred from 'simple-make/lib/Deferred'

export default function clean() {
  const deferred = new Deferred()
  rm('-rf', `src/GraphQL.Authorization/obj`)
  rm('-rf', `src/GraphQL.Authorization/bin`)

  rm('-rf', `src/GraphQL.Authorization.Tests/obj`)
  rm('-rf', `src/GraphQL.Authorization.Tests/bin`)

  deferred.resolve()

  return deferred.promise
}
