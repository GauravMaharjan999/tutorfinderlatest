var Banner = function (options) {
    var config = {
    };
    $.extend(config, options);
    var $ajaxCall = function (data, url, type, success, error) {
        $.ajax({
            type: type,
            async: true,
            url: url,
            data: data,
            success: success,
            error: error
        });
    };
    var $ajaxCallWithLoader = function (data, url, type, beforeSend, complete, success, error) {
        $.ajax({
            type: type,
            async: true,
            url: url,
            data: data,
            beforeSend: beforeSend,
            complete: complete,
            success: success,
            error: error
        });
    };
  

    var fValidation = function (formElement, rules, messages) {
        var form = $(formElement).validate({
            rules: rules,
            messages: messages,
            highlight: function (input) {
                $(input).parents('.form-group:eq(0)').addClass('has-error');
            },
            unhighlight: function (input) {
                $(input).parents('.form-group:eq(0)').removeClass('has-error');
            },
            errorPlacement: function (error, element) {
                $(element).parents('div:eq(0)').append(error);
            }
        });
        return form;
    };


    var bannerImageUploadCropHandler = function () {
        var jqXHRData;
        var hasImage = false;

        var setImagePlaceHolder = function () {
            $('#bannerPlaceholder').css({
                'border': '1px dashed black',
                'height': 420

            });
        };

        var ajaxUploadFileInit = function () {
           
            $('#file-upload-id').fileupload({
                url: '/api/v1/banner/upload',//'/admin/banner/uploadfile',
                dataType: 'json',
                add: function (e, data) {
                    jqXHRData = data;
                },
                done: function (event, data) {
                    var response = data.result;
                    if (response.Code==200) {
                        hasImage = true;
                        
                        $("#hf-uploaded-image-path").val(response.Data.filepath);
                        $("#crop-image-area").attr("src", response.Data.filepath + "?t=" + new Date().getTime());
                      
                    } else {
                        console.log('error uploading image');
                    }
                },
                fail: function (event, response) {
                    if (response.Message) {
                        alert(response.Message);
                    }
                }
            });
        };

        var uploadFile = function () {
            if (jqXHRData) {
                jqXHRData.submit();
            }
            return false;
        };
      


        var clearBannerElements = function () {
            $("#hf-uploaded-image-path").val('');
            $('#hf-cropped-image-path').val('');
            $("#crop-image-area").attr('style', "").attr("src", "");
            $('#content-div').hide();
            $(".kachuwa-form input,textarea").val("");
            $(".kachuwa-form select").val(0);
        }

      
        var getUploadedFileHeightWidth = function (input, callback) {
            var reader = new FileReader;
            reader.onload = function (e) {
                var image = new Image();
                image.src = reader.result;
                image.onload = function () {
                    var dimensions = {
                        height: image.height,
                        width: image.width
                    };
                    callback(dimensions);
                };
            };
            reader.readAsDataURL(input.files[0]);
        };

        var imageUploadCb = function (result) {
            // if (result.height >= imageCropHeight && result.width >= imageCropWidth) {
            uploadFile();
            //} else {
            //    alert("Please upload larger files.");
            //}
        }

        var initDraggableInBannerContent = function () {

           
        }


        var animateContent = function (cssClass) {
            var old_class = $("#content-div").attr('class');
            $("#content-div").addClass(cssClass).removeClass(old_class);
        }

        var setColorPicker = function () {
            $("#custom-content-color").off('input').on('input', function() {
                $('.bannerContent').css({ 'color': $(this).val() });
            });
            $("#custom-bg-color").off('input').on('input', function () {
                $('#bannerPlaceholder').css({ 'background-color': $(this).val() });
            });
            $("#custom-heading-color").off('input').on('input', function () {
                $('.bannerHeading').css({ 'color': $(this).val() });
            });
            //$("#custom-link-color").off('input').on('input', function () {
            //    $('.bannerLink').css({ 'color': $(this).val() });
            //});
        }

        var doAnimations = function (elems) {
            var animEndEv = 'webkitAnimationEnd animationend';
            elems.each(function () {
                var $this = $(this),
                    $animationType = $this.data('animation');
                    
                    $this.addClass($animationType).one(animEndEv, function () {
                    $this.removeClass($animationType);
                });
            });
        }



        var askUserForNewUpload = function () {
            swal({
                title: "Add banner",
                text: "Do you want to add more banners?",
                type: "",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes",
                cancelButtonText: "No",
                closeOnConfirm: true,
                closeOnCancel: false
            },
            function (isConfirm) {
                if (isConfirm) {
                    clearBannerElements();
                } else {
                    location.href = '/admin/banner';
                }
            });
        }

        var saveBanner = function (successCB) {
            var model = {
                KeyId: $('#BannerKeyId').val(),
                BannerId: $("#BannerId").val(),
                Image: $("#hf-uploaded-image-path").val(),
                IsVideo: $("input[name=IsVideo]").is(":checked"),
                VideoLink: $("input[name=VideoLink]").val(),
                EmbeddedVideoLink: $("input[name=EmbeddedVideoLink]").val(),
                HeadingText: $.trim($('input[name=HeadingText]').val()),
                Content: $.trim($('textarea[name=Content]').val()),
                Link: $.trim($('input[name=Link]').val()),
                HeadingTextAnimation: $('#BannerHeadingTextAnimation').val(),
                HeadingContentAnimation: $('#BannerContentAnimation').val(),
                LinkAnimation: $('#BannerLinkAnimation').val(),
                BannerAnimation: $('#BannerAnimation').val(),
                BannerContentPositionClass: $('#BannerContentLayout').val(),
                BannerHeadingColor: $("#custom-heading-color").val(),
                BannerContentColor: $("#custom-content-color").val(),
                BannerLinkColor: $("#custom-link-color").val(),
                BannerBackgroundColor: $('#custom-bg-color').val(),
                BannerImagePosition: $('#BannerImagePosition').val(),
                ImageHeight: $("input[name=ImageHeight]").val(),
                ImageWidth:$("input[name=ImageWidth]").val()
            }
            $ajaxCall(model, '/api/v1/banner/save',// '/admin/banner/save',
                'POST', function (response) {
                if (response.Code==200) {
                    alert('saved');
                    clearBannerElements();
                } 

            }, function () {

            });
        }

        var events = function () {

            setImagePlaceHolder();

            //setEditableElements();
            
            setColorPicker();

            $(document).off('input', 'input[name=HeadingText]').on('input', 'input[name=HeadingText]', function (e) {
               
            });
            $(document).off('click', '.fileinput-button').on('click', '.fileinput-button', function (e) {
                ajaxUploadFileInit();
            });

            $(document).off('change', '#file-upload-id').on('change', '#file-upload-id', function (e) {
                getUploadedFileHeightWidth(this, imageUploadCb);
            });

            $(document).off('change', '#BannerAnimation').on("change", '#BannerAnimation', function () {
                $('#crop-image-area').removeAttr('class');
                $('#crop-image-area').attr('class', 'animated').addClass($(this).val());
            });


            $(document).off('change', '#BannerHeadingTextAnimation').on("change", '#BannerHeadingTextAnimation', function () {
                $('#content-div h3').removeAttr('class');
                $('#content-div h3').attr('class','bannerHeading').addClass('editable').addClass('animated').addClass($(this).val());
            });

            $(document).off('change', '#BannerContentAnimation').on("change", '#BannerContentAnimation', function () {
                $('#content-div p:eq(0)').removeAttr('class');
                $('#content-div p:eq(0)').attr('class', 'bannerContent').addClass('editable').addClass('animated').addClass($(this).val());
            });

            $(document).off('change', '#BannerLinkAnimation').on("change", '#BannerLinkAnimation', function () {
                $('#content-div p:eq(1)').removeAttr('class');
                $('#content-div p:eq(1)').attr('class', 'bannerLink').addClass('editable').addClass('animated').addClass($(this).val());
            });

            $(document).off('change', '#BannerImagePosition').on("change", '#BannerImagePosition', function () {
                var cur_class = $('#crop-image-area').parent().attr('class');
                $('#crop-image-area').parent().removeClass(cur_class).addClass($(this).val());
            });

            $(document).off('change', '#BannerContentLayout').on('change', '#BannerContentLayout', function (e) {
              
                if ($(this).val() == 'custom-drag') {
                    animateContent($(this).val());
                    initDraggableInBannerContent();
                } else {
                    animateContent($(this).val());
                }
            });

            $(document).off('click', '.formsave').on('click',
                '.formsave',
                function() {
                    if ($('#BannerKeyId').val() == "" || $('#BannerKeyId').val() == null) {
                        alert("Please select banner key first");
                    } else if ($('#hf-uploaded-image-path').val() == null || $('#hf-uploaded-image-path').val() == "") {
                        alert("Please crop banner first !");
                    } else {
                        saveBanner(askUserForNewUpload);
                    }
                });

        }();

    }();

    var bannerManagement = function () {
        var loadBannerPreview = function (keyId) {
            var model = {
                BannerKeyId : keyId
            }

            $ajaxCall(model, '/api/v1/banner/preview',//'/admin/banner/getbannerpreview',
                'POST', function (response) {
                if (response.Code === 200) {


                    $('#myBannerPreviewModal').find('#carousel-example-generic').html('');

                    $('<ol class="carousel-indicators"></ol>').appendTo('#carousel-example-generic');
                    $('<div class="carousel-inner" role="listbox"></div>').insertAfter('.carousel-indicators');

                    for (var i = 0; i < response.Data.length; i++) {
                        var olhtml = '';
                        var activeClass = (i === 0) ? "class=active" : "";
                        olhtml += '<li data-target="#carousel-example-generic" data-slide-to="' + i + '"' + activeClass + '"></li>';
                        $('.carousel-indicators').append(olhtml);
                    }

                    $.each(response.Data, function(index,value) {
                        var html = '';
                        var activeClass = index === 0 ? "active" : "item";
                        
                        html +='<div class="item '+ activeClass +'" style="background-color:'+ value.BannerBackgroundColor +'">';
                        html +='<div class="row">';
                        html += '<div class="container">';

                        if (value.BannerImagePosition === 'text-right') {
                            html += '<div class="'+ value.BannerImagePosition + '">';
                            html += '<img data-animation="animated ' + value.BannerAnimation + '" src="' + value.BannerImage + '">';
                            html += '</div>';
                            html += '<div class="'+ value.BannerContentPositionClass + '">';
                            html += '<h3 style="color:'+ value.BannerHeadingColor +'" data-animation="animated ' + value.HeadingTextAnimation + '">' + value.HeadingText + '</h3>';
                            html += '<h4 style="color:' + value.BannerContentColor + '" data-animation="animated ' + value.HeadingContentAnimation + '">' + value.Content + '</h4>';
                            html += '</div>';
                        } else {
                            html += '<div class="' + value.BannerContentPositionClass+ '">';
                            html += '<h3 style="color:' + value.BannerHeadingColor + '" data-animation="animated ' + value.HeadingTextAnimation + '">' + value.HeadingText + '</h3>';
                            html += '<h4 style="color:' + value.BannerContentColor + '" data-animation="animated ' + value.HeadingContentAnimation + '">' + value.Content + '</h4>';
                            html += '</div>';
                            html += '<div class="' + value.BannerImagePosition + '">';
                            html += '<img data-animation="animated ' + value.BannerAnimation + '" src="' + value.BannerImage + '">';
                            html += '</div>';
                        }
                        
                        html +='</div>';
                        html +='</div>';
                        html +='</div>';
                        $('.carousel-inner').append(html);
                    });

                    var controls = '';
                    controls +='<a class="left carousel-control" href="#carousel-example-generic" role="button" data-slide="prev">';
                    controls +='<i class="fa fa-angle-left"></i><span class="sr-only">Previous</span>';
                    controls +='</a>';
                    controls +='<a class="right carousel-control" href="#carousel-example-generic" role="button" data-slide="next">';
                    controls +='<i class="fa fa-angle-right"></i><span class="sr-only">Next</span>';
                    controls +='</a>';

                    var z = $(controls);
                    z.insertAfter('.carousel-inner');

                    $('#myBannerPreviewModal').modal('show');
                    
                }
            }, function() {});


        };

        var deleteBannerItem = function (keyId) {
            var model = {
                BannerId: keyId
            }

            $ajaxCall(model, '/api/v1/banner/image/delete',// '/admin/banner/DeleteBannerItem',
                'POST', function (response) {
                if (response.Code == 200) {
                    $.toast({
                        heading: 'Success',
                        text: 'Banner deleted successfully',
                        showHideTransition: 'fade',
                        position: 'bottom-right',
                        stack: false,
                        icon: 'success'
                    });
                    location.reload();
                } else {
                    $.toast({
                        heading: 'Error',
                        text: 'Something went wrong :(',
                        showHideTransition: 'fade',
                        position: 'bottom-right',
                        stack: false,
                        icon: 'error'
                    });
                }
            }, function () { });
        }
        return {
            ShowPreview: loadBannerPreview,
            DeleteBannerItem: deleteBannerItem
        }
    }();

    var bannerKeyManagement = function () {
        var clearBannerKeyForm = function () {
            $('#BannerKeyId').val(0);
            $('#BannerKeyName').val('');
            $('#taDescription').val('');
        };
        var bindBannerKeys = function () {

        };
        var loadKeyManagementForm = function () {
            $('#myModal').modal('show');
        };

        var populateBannerKeys = function () {
            $ajaxCall('', '/api/v1/banner/keys',// '/admin/banner/GetBannerKeys',
                'POST', function (response) {
                if (response.Code == 200) {
                    $('#BannerKeyId').html('');
                    $('<option value="">--Select Key--</option>').appendTo('#BannerKeyId');
                    $.each(response.Data, function (index, value) {
                        var html = '';
                        html += '<option value="' + value.BannerKeyId + '">' + value.Name + '</option>';
                        $('#BannerKeyId').append(html);
                    });
                }
            }, function () { });
        }

        var saveBannerKey = function () {
            var model = {
                BannerKeyId: 0,
                Name: $('#BannerKeyName').val(),
                Description: $('#taDescription').val()
            }

            var formElement = $("#myModal").find('#frmBannerKey');
            var rules = {
                BannerKeyName: { required: true },
                Description: { required: true }
            }

            var messages = {
                BannerKeyName: { required: "*" },
                Description: { required: "*" }
            }

            var valForm = fValidation(formElement, rules, messages);

            if (valForm.form()) {
                $ajaxCall(model, '/api/v1/banner/key/save',//'/admin/banner/savebannerkey',
                    'POST', function (response) {
                        populateBannerKeys();
                    $('#myModal').modal('hide');
                        alert('key saved');
                    }, function() {
                        alert('error saving');
                    });
            }
        }


        var reloadBannerList = function() {
            $ajaxCall(model, '/api/v1/banner/list',//'/admin/banner/GetBannerList',
                'POST', function (response) {
                if (response.Code == 200) {
                    alert('jkfdjkj')
                } else {
                    
                }
            }, function () { });
        }

        var deleteBannerKey = function (keyId) {
            var model = {
                BannerKeyId: keyId
            }

            $ajaxCall(model, '/api/v1/banner/delete',//'/admin/banner/DeleteBanner',
                'POST', function (response) {
                if (response.Code == 200) {
                    $.toast({
                        heading: 'Success',
                        text: 'Key deleted successfully',
                        showHideTransition: 'fade',
                        position: 'bottom-right',
                        stack: false,
                        icon: 'success'
                    });
                    location.reload();
                    //reloadBannerList();
                } else {
                    $.toast({
                        heading: 'Error',
                        text: 'Something went wrong :(',
                        showHideTransition: 'fade',
                        position: 'bottom-right',
                        stack: false,
                        icon: 'error'
                    });
                }
            }, function () { });
        }
        var init = function() {
            populateBannerKeys();
        }();
        return {
            ShowForm: loadKeyManagementForm,
            BindBannerKeys: bindBannerKeys,
            ClearForm: clearBannerKeyForm,
            SaveBannerKey: saveBannerKey,
            DeleteKey: deleteBannerKey
        }
    }();


    var globalEvents = function () {

        $(document).off('click', '.btnAddBanner').on('click', '.btnAddBanner', function () {
            bannerKeyManagement.ShowForm();
        });

        $(document).off('click', '.btnCancelKeySave').on('click', '.btnCancelKeySave', function () {
            $('#myModal').modal('hide');
            bannerKeyManagement.ClearForm();
        });

        $(document).off('click', '.btnShowPreview').on('click', '.btnShowPreview', function () {
            var keyId = $(this).data('bannerkeyid');
            bannerManagement.ShowPreview(keyId);
        });


        $(document).off('click', '.btnProceed').on('click', '.btnProceed', function () {
            bannerKeyManagement.SaveBannerKey();
        });


        $(document).off('click', '.btnDeleteBannerKey').on('click', '.btnDeleteBannerKey', function () {
            if (confirm("Are you sure?")) {
                var keyId = $(this).data('bannerkey-id');
                bannerKeyManagement.DeleteKey(keyId);
            }
        });

        $(document).off('click', '.btnDeleteBannerItem').on('click', '.btnDeleteBannerItem', function () {
            if (confirm("Are you sure?")) {
                var keyId = $(this).data('banner-id');
                bannerManagement.DeleteBannerItem(keyId);
            }
        });

    }();

    return {};
};