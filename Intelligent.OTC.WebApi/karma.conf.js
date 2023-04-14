// Karma configuration
// Generated on Mon Jun 16 2014 15:04:49 GMT+1000 (AUS Eastern Standard Time)

module.exports = function(config) {
  config.set({

    // base path that will be used to resolve all patterns (eg. files, exclude)
    basePath: '',


    // frameworks to use
    // available frameworks: https://npmjs.org/browse/keyword/karma-adapter
    frameworks: ['jasmine'],


    // list of files / patterns to load in the browser
    files: [
    'Scripts/jquery-3.4.1.js',

    'Scripts/angular.js',
    'Scripts/angular-*.js',
    'Scripts/ng-grid.js',
    'Scripts/angular-ui/ui-bootstrap-tpls.js',
    'Scripts/bootstrap-datetimepicker.js',
    'Scripts/tinymce.js',
    'Scripts/console-sham.js',
    'Scripts/console-sham.min.js',
    'Scripts/angular-ui/angular-file-upload.js',
    'common/directives/datetimepicker.js',
    'common/directives/wcOverlay.js',
    'common/utils/Query.js',
    'common/directives/crud/crudButtons.js',
    'common/directives/crud/TestDirectives.js',
    'common/directives/crud/edit.js',
    'common/providers/crudRouteProvider.js',
    'common/services/i18nNotifications.js',
    'common/services/localizedMessages.js',
    'common/services/notifications.js',
    'common/services/breadscrumbsService.js',
    'common/services/modalService.js',
    'common/services/crudService.js',
    'common/services/Resource.js',
    'common/resources/contactProxy.js',
    'common/resources/customerProxy.js', 
    'config/constants.js',
    'demo/main.js',
    'demo/pagination.js',
    'demo/datepicker.js',
    'demo/grid.js',
    'demo/modal.js',
    'demo/tinyMCE.js',
    'demo/tooltips.js',
    'demo/uploaderFile.js',
    'app/customer/customer.js',
    'app/customer/contact/contact.js',

    'app/app.js',
    'test/unit/*.js'
    ],


    // list of files to exclude
    exclude: [
       'Scripts/*.min.js',
       'Scripts/angular-kendo*.js'
    ],


    // preprocess matching files before serving them to the browser
    // available preprocessors: https://npmjs.org/browse/keyword/karma-preprocessor
    preprocessors: {
    
    },


    // test results reporter to use
    // possible values: 'dots', 'progress'
    // available reporters: https://npmjs.org/browse/keyword/karma-reporter
    // reporters: ['progress', 'xml'],
    reporters: ['progress'],


    // web server port
    port: 9876,


    // enable / disable colors in the output (reporters and logs)
    colors: true,


    // level of logging
    // possible values: config.LOG_DISABLE || config.LOG_ERROR || config.LOG_WARN || config.LOG_INFO || config.LOG_DEBUG
    logLevel: config.LOG_INFO,


    // enable / disable watching file and executing tests whenever any file changes
    autoWatch: false,


    // start these browsers
    // available browser launchers: https://npmjs.org/browse/keyword/karma-launcher
    //browsers: ['PhantomJS'],
    browsers: ['PhantomJS'],

    // Continuous Integration mode
    // if true, Karma captures browsers, runs the tests and exits
    singleRun: true
  });
};
