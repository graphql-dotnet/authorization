import make from 'simple-make/lib/make'
import config from 'simple-make/lib/config'
import {
  compile,
  clean,
  dotnetPack,
  dotnetTest,
  projectVersion,
  restore,
  setVersion,
  version
} from './tasks'

config.name = '[authorization]'
config.format = seconds => `Done in ${seconds}s.`

const args = process.argv.slice(2)

const tasks = {
  artifacts: ['nuget'],
  compile: [clean, 'restore', compile],
  test: dotnetTest,
  version: [version],
  nuget: dotnetPack,
  restore,
  setVersion: () => setVersion(args[1]),
  'default': 'compile test',
  ci: 'version default artifacts'
}

make({ tasks })
