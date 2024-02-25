fx_version 'bodacious'
game 'gta5'



files {
	'dist/Client/*.dll',
	/*'ui/index.html',
	'ui/scripts/*.js',
	'ui/assets/*.png',
	'ui/assets/img/*.png',
	'ui/assets/img/backgrounds/*.png',

	'ui/assets/img/inventory/*.png',
	'ui/assets/img/hud/*.png',
	'ui/assets/icons/*.png',
	'ui/css/*.css',
	'ui/scss/*.css',
	'ui/hudscss/*.css',
	'ui/menucss/*.css',
	'ui/fonts/*.ttf',*/
	 -- CONFIG --
	    '*.json',
	    /*'config/*.json',
	    'utils/*.json',*/
	
}

ui_page 'ui/index.html'
client_script {
'dist/Client/*.net.dll',
'dist/Shared/*.net.dll',

}
--server_script 'dist/Server/*.net.dll'

author 'Cruso'
version '0.0.0'
description 'Script for FiveM Game Server'