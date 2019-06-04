import { exec } from 'shelljs'

export default function nugetRestore() {
  exec('dotnet restore src')
  return Promise.resolve()
}
