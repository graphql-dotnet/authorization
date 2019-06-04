import { exec, rm } from 'shelljs'
export default function clean() {
  rm('-rf', `src/GraphQL.Authorization/obj`)
  rm('-rf', `src/GraphQL.Authorization/bin`)

  rm('-rf', `src/GraphQL.Authorization.Tests/obj`)
  rm('-rf', `src/GraphQL.Authorization.Tests/bin`)

  return Promise.resolve()
}
