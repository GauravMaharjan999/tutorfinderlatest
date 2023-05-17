/// <binding AfterBuild='default' />
var gulp = require('gulp');
var sass = require('gulp-sass');
var sourcemaps = require('gulp-sourcemaps');
var bs = require('browser-sync').create();
var rename = require('gulp-rename');

 function compile_default() {
    return gulp.src('./themes/shared/default/sass/theme.scss')
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(sourcemaps.write('./maps'))
        .pipe(gulp.dest('./themes/shared/default/css'))
        .pipe(bs.reload({
            stream: true
        }));
}
function compile_ok() {
    return gulp.src('./themes/shared/ok/scss/style.scss')
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(sourcemaps.write('./maps'))
        .pipe(gulp.dest('./themes/shared/ok/css'))
        .pipe(bs.reload({
            stream: true
        }));
 }
function compile_ok_prod() {
    return gulp.src('./themes/shared/ok/scss/style.scss')
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('./themes/shared/ok/css'))
        .pipe(bs.reload({
            stream: true
        }));
}
gulp.task('compile_ok', compile_ok);
gulp.task('compile_ok_prod', compile_ok_prod);
gulp.task('compile_default', compile_default);
gulp.task('compile_default_prod', function () {
    return gulp.src('./themes/shared/default/sass/theme.scss')
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('./themes/shared/default/css'))
        .pipe(bs.reload({
            stream: true
        }));
});
gulp.task('copy:fa5', function () {
    return gulp.src('./node_modules/@fortawesome/fontawesome-free/**')
        .pipe(gulp.dest('./wwwroot/lib/fontawesome5'));
});
gulp.task('copy:chartjs', function () {
    return gulp.src('./node_modules/chart.js/dist/*')
        .pipe(gulp.dest('./wwwroot/lib/chart.js/dist'));
});
gulp.task('copy:jspdfautotable', function () {
    return gulp.src('./node_modules/jspdf-autotable/dist/*')
        .pipe(gulp.dest('./wwwroot/lib/jspdf-autotable/dist'));
});
gulp.task('copy:jspdf', function () {
    return gulp.src('./node_modules/jspdf/dist/*')
        .pipe(gulp.dest('./wwwroot/lib/jspdf/dist'));
});
gulp.task('copy:signalr', function () {
    return gulp.src('./node_modules/@aspnet/signalr/dist/browser/*')
        .pipe(gulp.dest('./wwwroot/lib/signalr/dist'));
});
gulp.task('copy:signalr-msgpack', function () {
    return gulp.src('./node_modules/@aspnet/signalr-protocol-msgpack/dist/browser/*')
        .pipe(gulp.dest('./wwwroot/lib/signalr/signalr-protocol-msgpack/dist'));
});
gulp.task('copy:sweetalert', function () {
    return gulp.src('./node_modules/sweetalert/dist/*')
        .pipe(gulp.dest('./wwwroot/lib/sweetalert/dist'));
});



gulp.task('compile_admin', function () {
    return gulp.src('./themes/shared/admin/sass/theme.scss')
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('./themes/shared/admin/css'))
        .pipe(bs.reload({
            stream: true
        }));
});
gulp.task('compile_admin_prod', function () {
    return gulp.src('./themes/shared/admin/sass/theme.scss')
        .pipe(sass({ outputStyle: 'compressed' }).on('error', sass.logError))
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest('./themes/shared/admin/css'))
        .pipe(bs.reload({
            stream: true
        }));
});

//Watch task
gulp.task('watch_admin_theme', function () {
    gulp.watch('./themes/shared/admin/**/*.scss', gulp.series('compile_admin', 'compile_admin_prod'));
});

//Watch task
gulp.task('watch_default_theme', function () {
    gulp.watch('./themes/shared/default/**/*.scss', gulp.series('compile_default', 'compile_default_prod'));
});


function compile_ok() {
    return gulp.src('./themes/shared/ok/scss/style.scss')
        .pipe(sourcemaps.init())
        .pipe(sass().on('error', sass.logError))
        .pipe(sourcemaps.write('./maps'))
        .pipe(gulp.dest('./themes/shared/ok/css'))
        .pipe(bs.reload({
            stream: true
        }));
}
gulp.task('compile_ok', compile_ok);
gulp.task('watch_ok', function () {
    gulp.watch('./themes/shared/ok/**/*.scss', gulp.series('compile_ok'));
});
gulp.task('watch', gulp.series('watch_admin_theme', 'watch_default_theme'));
gulp.task('watch_ok', gulp.series('watch_ok'));
gulp.task('default', gulp.parallel('copy:fa5', 'copy:chartjs', 'copy:jspdf', 'copy:jspdfautotable', 'copy:signalr', 'copy:signalr-msgpack', 'copy:sweetalert', 'compile_default', 'compile_default_prod', 'compile_ok', 'compile_admin',
    'compile_admin_prod',
    'compile_ok',
    'compile_ok_prod'));//, ]);

gulp.task('dev', gulp.parallel('copy:fa5', 'copy:chartjs', 'copy:jspdf', 'copy:jspdfautotable', 'copy:signalr', 'copy:signalr-msgpack', 'copy:sweetalert', 'compile_default', 'compile_ok', 'compile_admin','compile_ok', 'watch'));//, ']);







 