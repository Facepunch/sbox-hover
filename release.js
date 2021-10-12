var fs = require( "fs" );
var archiver = require( "archiver" );
var gitignore = fs.readFileSync( ".gitignore" ).toString().split( "\n" );

for ( var i in gitignore ) {
    var line = gitignore[i];
    console.log( line );
}

var output = fs.createWriteStream( "release.zip" );
var archive = archiver( "zip", {
    zlib: { level: 9 }
});

output.on( "close" , function () {
    console.log(archive.pointer() + ' total bytes');
    console.log( "archiver has been finalized and the output file descriptor has closed." );
});

archive.on( "error" , function(err) {
    throw err;
});

archive.pipe( output );

archive.glob( "**/{*.cs,*.scss,*.vmdl_c,*.vmat_c,*.vsnd_c,*.sound_c,*.html,*.vfx,*.png,*.vpcf_c,*.vtex_c,.addon,*.fgd,!.git}", {
    ignore: [
        "_bakeresourcecache/**/*.*"
    ]
});

archive.finalize();
