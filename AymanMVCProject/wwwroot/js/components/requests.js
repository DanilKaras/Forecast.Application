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
                type:'Post',
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
                type:'Post',
                data: data,
                success: function (data) {
                    //onSuccessLoad(data);
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
            type:'Get',
            success: function (data) {
                builder.showRequestForToday(data);
                utils.loaderHide();
            },
            error: function (error) {
                utils.loaderHide();
            }
        })
    };
    
    
    
    var testPython = function () {
        $.ajax({
            url: utils.domain + '/TestLink',
            type:'Get',
            success: function (data) {
                alert('success!');
                utils.loaderHide();
            },
            error: function (error) {
                alert(error.responseJSON.message);
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
        testPython: testPython
    };
})();