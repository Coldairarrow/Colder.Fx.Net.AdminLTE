//搜索表格
function searchGrid(searchBtnObj, gridSelector) {
    var $wrapper = $(searchBtnObj).closest("div.search_wrapper");
    if (!$wrapper || !$wrapper.length) {
        return;
    }

    var params = $wrapper.getValues();
    $(gridSelector).datagrid("load", params);
}

//搜索树表格
function searchTreeGrid(searchBtnObj, gridSelector) {
    var $wrapper = $(searchBtnObj).closest("div.search_wrapper");
    if (!$wrapper || !$wrapper.length) {
        return;
    }

    var params = $wrapper.getValues();
    $(gridSelector).treegrid("load", params);
}

//加载动画
function loading(isLoading) {
    var loading = true;
    if (typeof (isLoading) != 'undefined')
        loading = isLoading;
    if (loading) {
        $('<div id="loadingMask" class="datagrid-mask"></div>')
            .css({
                display: "block",
                'z-index': 998,
                width: "100%",
                height: '100%'
            })
            .appendTo("body");
        $("<div id=\"loadingMaskMsg\" class=\"datagrid-mask-msg\"></div>")
            .html("加载中，请稍候。。。")
            .css({
                display: "block",
                'z-index': 999,
                left: ($(document.body).outerWidth(true) - 190) / 2,
                top: ($(window).height() - 45) / 2
            })
            .appendTo("body");
    } else {
        $("#loadingMask").remove();
        $("#loadingMaskMsg").remove();
    }
}

//处理请求返回数据
function accessResJson(resJson) {
    if (resJson.Success) {
        dialogMsg('操作成功！');
    } else {
        dialogError('操作失败！详情:{0}'.format(resJson.Msg));
    }
}

//重定向
function redirect(url, param) {
    if (!url)
        return;

    var _param = param || {};
    var formId = 'redirectForm' + new Date().getTime();
    var formHtml = "<form id='{0}' method='post' action='{1}'>".format(formId, url);

    Object.keys(_param).forEach(function (aProperty) {
        if (_param[aProperty]) {
            formHtml += "<input type='hidden' name='{0}' value='{1}'/>".format(aProperty, _param[aProperty].toString());
        }
    });

    formHtml += '</form>';
    $(formHtml).appendTo('body');

    $('#' + formId).submit();
}

//初始化日期控件为年月
function init_yearMonth(id, value) {
    var db = $('#' + id);
    db.datebox({
        value: value,
        width: 100,
        editable: true,
        prompt: '选择年月',
        validType: [],
        //readonly: false,
        onShowPanel: function () {//显示日趋选择对象后再触发弹出月份层的事件，初始化时没有生成月份层
            span.trigger('click'); //触发click事件弹出月份层
            if (!tds) setTimeout(function () {//延时触发获取月份对象，因为上面的事件触发和对象生成有时间间隔
                tds = p.find('div.calendar-menu-month-inner td');
                tds.click(function (e) {
                    e.stopPropagation(); //禁止冒泡执行easyui给月份绑定的事件
                    var year = /\d{4}/.exec(span.html())[0]//得到年份
                        , month = parseInt($(this).attr('abbr'), 10); //月份，这里不需要+1
                    db.datebox('hidePanel')//隐藏日期对象
                        .datebox('setValue', year + '-' + month); //设置日期的值
                });
            }, 0);
            yearIpt.unbind();//解绑年份输入框中任何事件
            $(yearIpt).attr('readonly', true);//年份只读
            $(yearIpt).css('border-color', 'white');//边框去掉
        },
        parser: function (s) {
            if (!s) return new Date();
            var arr = s.split('-');
            return new Date(parseInt(arr[0], 10), parseInt(arr[1], 10) - 1, 1);
        },
        formatter: function (d) { return d.getFullYear() + '-' + (d.getMonth() + 1);/*getMonth返回的是0开始的，忘记了。。已修正*/ }
    });
    var p = db.datebox('panel'), //日期选择对象
        tds = false, //日期选择对象中月份
        aToday = p.find('a.datebox-current'),
        yearIpt = p.find('input.calendar-menu-year'),//年份输入框
        //显示月份层的触发控件
        span = aToday.length ? p.find('div.calendar-title span') ://1.3.x版本
            p.find('span.calendar-text'); //1.4.x版本
    if (aToday.length) {//1.3.x版本，取消Today按钮的click事件，重新绑定新事件设置日期框为今天，防止弹出日期选择面板
        aToday.unbind('click').click(function () {
            var now = new Date();
            db.datebox('hidePanel').datebox('setValue', now.getFullYear() + '-' + (now.getMonth() + 1));
        });
    }
}

//显示大图
function showBigImg(url) {
    top.dialogOpen({
        id: 'ShowBigImg',
        title: '添加数据',
        width: 600,
        height: 600,
        url: rootUrl + 'Base_SysManage/Common/ShowBigImg?url={0}'.format(url),
    });
}

//获取图片构造的Html
function getImgHtml(imgs) {
    var html = '';
    (imgs || '').split(',').forEach(function (item, index) {
        if (item == '')
            return;
        var br = '';
        if (index != 0)
            br = '<br />';
        html += '{0}<a href="javascript:;" onclick="showBigImg(\'{1}\')"><img src="{1}" style="width:100px;height:100px" /></a>'.format(br, item);
    });

    return html;
}