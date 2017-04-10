var gulp = require('gulp');

gulp.task('build.webpack', () => {
    var webpack = require('webpack-stream');
    var config = require('./webpack.config.js');

    return gulp.src('ClientApp/app/main.ts')
        .pipe(webpack(config))
        .pipe(gulp.dest('wwwroot/'));
});

gulp.task('watch.webpack', () => {
    var webpack = require('webpack-stream');
    var config = require('./webpack.config.js');

    config.watch = true;

    return gulp.src('ClientApp/app/main.ts')
        .pipe(webpack(config))
        .pipe(gulp.dest('wwwroot/'));
});

gulp.task('watch', ['watch.webpack']);

gulp.task('default', ['build.webpack']);