fx_version 'cerulean'  -- or bodacious, but cerulean is recommended for NUI

game 'gta5'

-- .NET scripts
client_script 'Client/*.net.dll'
server_script 'Server/*.net.dll'

-- NUI UI
ui_page 'html/index.html'

files {
  'html/index.html',
  'html/style.css',
  'html/app.js',
  'html/images/*.webp',
  'Client/Newtonsoft.Json.dll',
  'Client/Data/*.json'
}


author 'code-vine'
version '0.0.1'
description 'Vehicle Menu with NUI and C#'
