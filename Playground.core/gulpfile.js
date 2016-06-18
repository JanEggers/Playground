var gulp = require('gulp');
var clean = require('gulp-clean');

var destPath = './wwwroot/lib/';

// Delete the dist directory
gulp.task('clean', function () {
    return gulp.src(destPath)
        .pipe(clean());
});

gulp.task("node2lib", () => {
    return gulp.src([
            '@angular/**',
            'rxjs/**',
            'systemjs/**',
            'zone.js/**',
            'core-js/**'
    ], {
        cwd: "node_modules/**"
    })
        .pipe(gulp.dest(destPath));
});

gulp.task('watch', ['watch.ts']);

gulp.task('watch.ts', [], function () {
    return gulp.watch('scripts/*.ts', ['ts']);
});

gulp.task('default', ['node2lib']);