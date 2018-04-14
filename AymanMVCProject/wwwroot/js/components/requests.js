var requests = (function () {
    
    var updateAssets = function () {
        utils.loaderShow();
        $.ajax({
            url: utils.updateAssets,
            type: 'GET',
            success: function () {
                $('.selectpicker option').remove();
                selecter();
            },
            error: function (error) {
                utils.loaderHide();
                alert(error.statusText);
            }
        });
    };
    
    var selecter = function () {
        $.ajax({
            url: utils.symbolsList,
            type: 'GET',
            success: function (data) {
                if (data.symbols){
                    var picker = $(".selectpicker");
                    var jsonData = data.symbols;
                    for (var i = 0; i < jsonData.length; i++) {
                        picker.append('<option value="' + jsonData[i] + '">' + jsonData[i] + '</option>')
                    }
                    picker.selectpicker('refresh');
                }
                utils.loaderHide();
            },
            error: function (error) {
                utils.loaderHide();
            }
        })
    };
    
    
    var sendToServerManual = function (data) {
        if (data){
            $.ajax({
                url: utils.manualForecast,
                type:'POST',
                data: data,
                success: function (data) {
                    onSuccessLoad(data);
                    utils.loaderHide();
                    
                },
                error: function (error) {
                    utils.loaderHide();
                    var message = error.responseJSON.message;
                    if(error.responseJSON.requestCount){
                        message += " Made requests: "+ error.responseJSON.requestCount;
                    }
                    alert(message);
                }
            })
        }
        else{
            toastr.error("Cannot send data to server");
            utils.loaderHide();
        }
    };

    var sendToServerAuto = function (data) {
        if(data){
            $.ajax({
                url: utils.autoForecastPost,
                type:'POST',
                data: data,
                success: function (data) {
                    onSuccessLoadAuto(data);
                    utils.loaderHide();
                },
                error: function (error) {
                    utils.loaderHide();
                    var message = error.responseJSON.message;
                    if(error.responseJSON.requestCount){
                        message += " Made requests: "+ error.responseJSON.requestCount;
                    }
                    alert(message);
                }
            })
        }  
        else{
          toastr.error("Cannot send data to server");
          utils.loaderHide();
        }
    };
    
    var requestCount = function () {
        $.ajax({
            url: utils.requestForToday,
            type:'GET',
            success: function (data) {
                builder.showRequestForToday(data);
                utils.loaderHide();
            },
            error: function (error) {
                utils.loaderHide();
            }
        })
    };
    
    var showForecastElements = function (data) {
        $.ajax({
            url: utils.getForecastParts,
            type:'GET',
            data: data,
            success: function (data) {
                onSuccessLoadForecastElements(data);
                utils.loaderHide();
            },
            error: function (error) {
                utils.loaderHide();
            }
        })
    };
    
    var latestAssets = function () {
        $.ajax({
            url: utils.getLatestAssets,
            type:'GET',
            success: function (data) {
                onSuccessLoadAuto(data);
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.responseJSON.message);
                utils.loaderHide();
            }
        })
        
    };

    var instantForecast = function () {
        utils.loaderShow();
        $.ajax({
            url: utils.instantForecast,
            type:'GET',
            success: function (data) {
                if (data){
                    builder.instantForecast(data);
                }
                else{
                    alert('No BTC data!')
                }
            },
            error: function (error) {
                utils.loaderHide();
                alert(error.responseJSON.message);
                
            }
        })
    };
    
    var testPython = function () {
        $.ajax({
            url: utils.getForecastParts,
            type:'GET',
            success: function (data) {
                alert('success!');
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.responseJSON.message + 'Requests: ' +  error.responseJSON.requestCount);
                utils.loaderHide();
            }
        })
    };
    
    return {
        updateAssets: updateAssets,
        selecter: selecter,
        sendToServerManual: sendToServerManual,
        requestCount: requestCount,
        sendToServerAuto: sendToServerAuto,
        testPython: testPython,
        showForecastElements: showForecastElements,
        latestAssets: latestAssets,
        instantForecast: instantForecast
    };
})();