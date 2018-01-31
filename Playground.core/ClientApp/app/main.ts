
import 'zone.js';
import 'core-js';

import { platformBrowserDynamic } from './vendor';
import { AppModule } from './app.module';

const platform = platformBrowserDynamic();
platform.bootstrapModule(AppModule).catch(console.error);