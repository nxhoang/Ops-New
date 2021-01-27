
var Corp = getUrlParameter('corporation');
var Dept = getUrlParameter('department');
var FileNameSys = getUrlParameter('fileNameSys');
var FileId = getUrlParameter('fileId');
var RegisterId = $("#hdUsername").val();
var isChildren = 0;

var RegisterName = $("#divUserName").text();

function PlayVideoComment() {

    var src = GetVideoLink(Corp, Dept, FileNameSys);
    //var src = 'http://video.pungkookvn.com:8888/api/Media/Play?fol=UNI0037LRG001001&f=58X3YVwxgBqqvgU.mp4'; 
    $("#videoPreview").attr("src", src);

    var myVid = document.getElementById('videoPreview');

    //jqGridComment(FileId);
}

function GetVideoByTimeComment() {

    $("#comments-container .content").click(function (e) {

        var id = $(this).closest('li').attr('data-id');
        var videoTime = parseInt(GetTimeVideoById(FileId, id));
        var myVid = document.getElementById('videoPreview');
        myVid.currentTime = videoTime;
        $('#txtVideoTime').val(videoTime);
    });
}

function GetTimePause() {
    var time;
    $("#videoPreview").on("pause", function (e) {
        time = Math.floor(e.target.currentTime);
        $('#txtVideoTime').val(time);
    });
}

function GetTimeVideoById(fileId, commentId) {
    var timeVideo;
    $.ajax({
        url: "/Ops/GetTimeVideoById",
        async: false,
        type: "POST",
        data: JSON.stringify({ fileId: fileId, commentId: commentId }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            timeVideo = res;
        },
        error: function (jqXhr, status, errorThrown) {
            timeVideo = null;
        }
    });
    return timeVideo;
}

//Star Video Link by jquery comments
function JqueryComments() {
    $('#comments-container').comments({

        profilePictureURL: '/assets/jquery-comments/image/icons.jpg',
        //profilePictureURL: 'http://203.113.151.204:8080/BETAPDM/User/' + RegisterId + '.jpg',
        enableAttachments: true,
        enableHashtags: true,
        enableUpvoting: false,
        enableAttachments: false,
        postCommentOnEnter: true,
        enableEditing: true,

        //Load comments
        getComments: function (success, error) {
            var comments = GetCommentJquery(FileId);
            var commentsArray = [];

            for (var i = 0; i < comments.length; i++) {

                var currentUser = "";
                if (comments[i].CreatorId == RegisterId) {
                    currentUser = true;
                }
                else currentUser = false;

                var createDate = new Date(parseInt(comments[i].CreateDate.replace("/Date(", "").replace(")/")));
                var modifyDate = new Date(parseInt(comments[i].ModifyDate.replace("/Date(", "").replace(")/")));
               
                //var imageUrl = 'http://203.113.151.204:8080/BETAPDM/User/' + comments[i].CreatorId + '.jpg';
                var userImage = '/assets/jquery-comments/image/icons.jpg';

                //var userInfo = GetSexUserInforByUserName(RegisterId);
                //var name = userInfo.usmt.Sex;
                //var imgUrl = check(imageUrl);
                //var image = new Image();
                //image.src = imageUrl;
                //if (image.width == 0) {
                //    console.log(image.width);
                //    //alert("no image");
                //    //userImage = '/assets/jquery-comments/image/icons-male.jpg';
                //}
                //else {
                //    userImage = 'http://203.113.151.204:8080/BETAPDM/User/' + comments[i].CreatorId + '.jpg';
                //}
                    
                var videoTime = comments[i].ComAtSecond;
                var comContent;
                if (videoTime == 0 || videoTime == null) {
                    comContent = comments[i].ComContent;
                }
                else {
                    comContent = comments[i].ComContent + ' [' + videoTime + 's]';
                }

                var objcmt = {
                    id: comments[i].CommentId, parent: comments[i].ParentId,
                    created: createDate.toDateString(), modified: modifyDate.toDateString(),
                    content: comContent, creator: comments[i].CreatorId, fullname: comments[i].UserName,
                    file_url: comments[i].FileURL, file_mime_type: comments[i].FileType, created_by_current_user: currentUser,
                    upvote_count: comments[i].UpvoteCount, user_has_upvoted: comments[i].UserHasUpvote == 1 ? true : false,
                    file: comments[i].FileName, profile_picture_url: userImage
                };

                commentsArray.push(objcmt);

            }
            success(commentsArray);
        },

        //Add comment
        postComment: function (commentJSON, success, error) {
            var cmtId = GetCommentId(FileId);
            var timeVideo = $('#txtVideoTime').val();

            var myVid = document.getElementById('videoPreview');
            var durationVideo = Math.floor(myVid.duration);
            if (timeVideo > durationVideo) {
                ShowMessage("Video pause", "Time of comment can not be bigger than time of video", ObjMessageType.Info);
                return null;
            }

            if (timeVideo < 0) {
                ShowMessage("Video pause", "Time of comment can not be less than 0", ObjMessageType.Info);
                return null;
            }

            var objcmt = {
                FileId: FileId, CommentId: cmtId, ParentId: commentJSON.parent,
                CreateDate: commentJSON.created, ModifyDate: commentJSON.modified, ComContent: commentJSON.content,
                CreatorId: RegisterId, FileURL: commentJSON.file_url, FileType: commentJSON.file_mime_type,
                UpvoteCount: commentJSON.upvote_count, UserHasUpvote: commentJSON.user_has_upvoted == false ? 0 : 1,
                ComAtSecond: timeVideo
            };
            PostComment(objcmt);
            commentJSON.fullname = RegisterName.replace(/"/g, "");
            if (timeVideo != '') {
                commentJSON.content = commentJSON.content + ' [' + timeVideo + 's]';
            }
            location.reload();
            success(commentJSON);
           
        },

        //Edit comment
        putComment: function (commentJSON, success, error) {
            var timeVideo = $('#txtVideoTime').val();
            var myVid = document.getElementById('videoPreview');
            var durationVideo = Math.floor(myVid.duration);

            if (timeVideo > durationVideo) {
                ShowMessage("Video pause", "Time of comment can not be bigger than time of video", ObjMessageType.Info);
                return null;
            }

            if (timeVideo < 0) {
                ShowMessage("Video pause", "Time of comment can not be less than 0", ObjMessageType.Info);
                return null;
            }
            var objcmt = { FileId: FileId, CommentId: commentJSON.id, ComContent: commentJSON.content, ComAtSecond: timeVideo };

            EditComment(objcmt);
            if (timeVideo != '') {
                commentJSON.content = commentJSON.content + ' [' + timeVideo + 's]';
            }
            location.reload();
            success(commentJSON);
            
        },

        //Delete comment
        deleteComment: function (commentJSON, success, error) {
            var objcmt = { FileId: FileId, CommentId: commentJSON.id };

            DeleteComment(objcmt);
            success();
        },

        upvoteComment: function (commentJSON, success, error) {
            var objcmt = {
                FileId: FileId, CommentId: commentJSON.id, UpvoteCount: commentJSON.upvote_count,
                UserHasUpvote: commentJSON.user_has_upvoted == false ? 0 : 1
            };

            UpvoteComment(objcmt)
            success(commentJSON);
        },

        uploadAttachments: function (commentArray, success, error) {

            var responses = 0;
            var successfulUploads = [];

            var serverResponded = function () {
                responses++;

                // Check if all requests have finished
                if (responses == commentArray.length) {

                    // Case: all failed
                    if (successfulUploads.length == 0) {
                        error();

                        // Case: some succeeded
                    } else {
                        success(successfulUploads);
                    }
                }
            }

            $(commentArray).each(function (index, commentJSON) {
                var cmtId = GetCommentId(FileId);
                var objcmt = {
                    FileId: FileId, CommentId: cmtId, ParentId: commentJSON.parent,
                    CreateDate: commentJSON.created, ModifyDate: commentJSON.modified,
                    CreatorId: RegisterId, FileURL: commentJSON.file_url, FileType: commentJSON.file_mime_type,
                    fileName: commentJSON.file.name
                };
                UploadAttachments(objcmt);
            });

            success(commentArray);
        },
        createCommentElement: function (commentModel) {
            var x = commentModel.id;
            alert(x)
        }

    });
}

function GetCommentJquery(fileId) {
    var commentsArray;
    $.ajax({
        url: '/Ops/GetCommentJquery/',
        async: false,
        type: "POST",
        data: JSON.stringify({ fileId: fileId }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            commentsArray = res;
        },
        error: function (jqXhr, status, errorThrown) {
            commentsArray = null;
        }
    });
    return commentsArray;
}

function GetCommentId(fileId) {
    var maxId;
    $.ajax({
        url: "/Ops/GetCommentId",
        async: false,
        type: "POST",
        data: JSON.stringify({ fileId: FileId }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            maxId = res;
        },
        error: function (jqXhr, status, errorThrown) {
            maxId = null;
        }
    });
    return maxId;
}

function PostComment(comment) {
    $.ajax({
        url: '/Ops/PostComment/',
        type: "POST",
        data: JSON.stringify({ comment: comment }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {           
        },
        error: function (jqXhr, status, errorThrown) {
            return null;
        }
    });
}

function EditComment(comment) {
    $.ajax({
        url: '/Ops/EditComment/',
        type: "POST",
        data: JSON.stringify({ comment: comment }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
        },
        error: function (jqXhr, status, errorThrown) {
            return null;
        }
    });
}

function UpvoteComment(comment) {
    $.ajax({
        url: '/Ops/UpvoteComment/',
        type: "POST",
        data: JSON.stringify({ comment: comment }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
        },
        error: function (jqXhr, status, errorThrown) {
            return null;
        }
    });
}

function DeleteComment(comment) {
    $.ajax({
        url: '/Ops/DeleteComment/',
        type: "POST",
        data: JSON.stringify({ comment: comment }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
        },
        error: function (jqXhr, status, errorThrown) {
            return null;
        }
    });
}

function UploadAttachments(comment) {
    $.ajax({
        url: '/Ops/UploadAttachments/',
        type: "POST",
        data: JSON.stringify({ comment: comment }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
        },
        error: function (jqXhr, status, errorThrown) {
            return null;
        }
    });
}

function CheckUserImageExists(imageUrl, callBack) {
    var imageData = new Image();
    imageData.onload = function () {
        callBack(true);
    };
    imageData.onerror = function () {
        callBack(false);
    };
    imageData.src = imageUrl;
}

function GetSexUserInforByUserName(userName) {
    var userInfo;
    $.ajax({
        url: '/Account/GetUserInforByUserName/',
        async: false,
        type: "POST",
        data: JSON.stringify({ userName: userName }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            userInfo = res;
        },
        error: function (jqXhr, status, errorThrown) {
            userInfo = null;
        }
    });
    return userInfo;
}

//End

function EventBtnClick() {
    //Add comment
    $("#btnAddComment").click(function () {

        var commentNote = $("#txtComment").val();

        var d = new Date($.now());
        var dateTime = d.getDate() + "/" + (d.getMonth() + 1) + "/" + d.getFullYear() + " " + d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();

        var commentId = GetMaxCommentId(FileId);

        //Add comment
        var videoComment = { FileId: FileId, CommentId: commentId, CommentNote: commentNote, CommentDate: dateTime, RegisterId: RegisterId };
        AddVideoComment(videoComment);

        var data = { fileId: FileId };
        ReloadJqGrid2LoCal("tbComment", data);

        $("#txtComment").val('');
    });
}

function EventPressEnterAddComment() {
    $("#txtComment").keypress(function (e) {
        var key = e.which;
        if (key === 13) {
            //Press enter key
            $("#btnAddComment").click();
        }
    });
}

function GetMaxCommentId(fileId) {
    var maxId;
    $.ajax({
        url: "/Ops/GetMaxCommentId",
        async: false,
        type: "POST",
        data: JSON.stringify({ fileId: FileId }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            maxId = res;
        },
        error: function (jqXhr, status, errorThrown) {
            maxId = null;
        }
    });
    return maxId;
}

function GetVideoLink(corp, dept, fileName) {
    var link;
    $.ajax({
        url: "/Ops/GetVideoLink",
        async: false,
        type: "POST",
        data: JSON.stringify({ corp: corp, dept: dept, fileName: fileName }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            link = res;
        },
        error: function (jqXhr, status, errorThrown) {
            link = null;
        }
    });
    return link;
}

function AddVideoComment(videoComment) {
    var addStatus;
    $.ajax({
        url: "/Ops/AddVideoComment",
        async: false,
        type: "POST",
        data: JSON.stringify({ videoComment: videoComment }),
        dataType: "json",
        contentType: "application/json",
        success: function (res) {
            if (res === Success) {
                addStatus = true;
                ShowMessageOk("001", SmsFunction.Add, MessageType.Success, MessageContext.Add, ObjMessageType.Info);

            } else {
                addStatus = false;
                ShowMessageOk("010", SmsFunction.Check, MessageType.Error, MessageContext.InvalidData, MessageTypeAlert);
            }
        },
    });
    return addStatus;
}

function jqGridComment(fileId) {
    $("#tbComment").jqGrid({
        url: '/OPS/GetVideoComment',
        datatype: "json",
        postData: {
            fileId: fileId
        },
        width: null,
        shrinkToFit: false,
        height: 250,
        colModel: [
            { name: 'CommentNote', index: 'CommentNote', label: 'Comment Note', width: 380 },
            {
                name: 'RegisterId', index: 'RegisterId', label: 'Register Name', width: 200,
                formatter: function (cellvalue, option, rowObject) {
                    return RegisterName;
                }
            },
            {
                name: 'CommentDate', index: 'CommentDate', label: 'Comment Date', width: 150, formatter: "date",
                //formatter: "date", formatoptions: { newformat: "ISO8601Long" }
                formatoptions: { srcformat: "d-m-Y H:i:s", newformat: "d-m-Y H:i:s" },

            },
            { name: 'FileId', index: 'FileId', hidden: true },
            { name: 'CommentId', index: 'CommentId', hidden: true },
        ],
        rowList: [10, 20, 30],
        pager: '#pagerComment',
        sortname: 'CommentNote',
        viewrecords: true,
        loadonce: true,
        multiselect: false,
        sortorder: "desc",
        caption: "Comments",
        gridview: true,
        autowidth: false,
        gridComplete: function () {
            setTimeout(function () {
                window.updatePagerIcons();
            }, 0);
        }
    });
}