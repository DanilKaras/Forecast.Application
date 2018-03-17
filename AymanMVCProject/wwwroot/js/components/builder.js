var builder = (function () {
    
    var table = function (table) {
        if (table){
            
            var $content = '';
            for(var i = 0; i < table.length; i++)
            {
                $content += "<tr><td>" +
                    table[i].id+"</td><td>"+
                    table[i].ds+"</td><td>"+
                    table[i].yhat+"</td><td>"+
                    table[i].yhatLower+"</td><td>"+
                    table[i].yhatUpper+"</td></tr>";
            }
            $('#table-content').html($content);
        }
        else{
            toastr.error("no data from server")
        }
    };
    
    var imgForecast = function (picPath) {
        if(picPath){
            var imgForecast = $('<img />', {
                id: 'forecast',
                src: picPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#forecast-place').html(imgForecast);
        }
        else{
            toastr.error("No forecast image")
        }
        
    };
    
    var imgComponents = function (picPath) {
        if(picPath){
            var imgComponents = $('<img />', {
                id: 'components',
                src: picPath,
                class: "img-responsive",
                alt: 'Cinque Terre'
            });
            $('#components-place').html(imgComponents);
        } else {
            toastr.error("No components image");
        }
       
    };
    
    var assetName = function (assetName) {
        if(assetName){
            $('#asset-name').html(assetName);
        }else{
            toastr.error("No asset name");
        }
        
    };
    
    var toastrAlert = function (requestNum) {
        if(requestNum){
            var $toastrMessage = 'You made '+ requestNum +' requests today!';
            if(requestNum < 800)
            {
                toastr.success($toastrMessage);
            }
            else if(800 <= requestNum < 950){
                toastr.warning($toastrMessage);
            }
            else{
                toastr.error($toastrMessage);
            }
        } else {
            toastr.warning("No number of requests per day")
        }
    };
    
    var toastrConfig = function (){
        toastr.options = {
            "closeButton": false,
            "debug": false,
            "newestOnTop": false,
            "progressBar": true,
            "positionClass": "toast-bottom-right",
            "preventDuplicates": false,
            "onclick": null,
            "showDuration": "300",
            "hideDuration": "1000",
            "timeOut": "5000",
            "extendedTimeOut": "1000",
            "showEasing": "swing",
            "hideEasing": "linear",
            "showMethod": "fadeIn",
            "hideMethod": "fadeOut"
        };
    };
    
    return {
        table: table,
        imgForecast: imgForecast,
        imgComponents: imgComponents,
        assetName: assetName,
        toastrAlert: toastrAlert,
        toastrConfig: toastrConfig
    };
})();