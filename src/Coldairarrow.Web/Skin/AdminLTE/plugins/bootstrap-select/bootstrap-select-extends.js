/*
 拓展selectpicker,使其支持远程数据
 用法:
$('#roleList').selectpicker({
    url: '/Base_SysManage/Base_SysRole/GetDataList_NoPagin',
    valueField: 'RoleId',
    textField: 'RoleName',
});
 */
(function ($) {
    if (!$.fn._selectpicker) {
        $.fn._selectpicker = $.fn.selectpicker;
    }
    $.fn.selectpicker = function (options, param) {
        var _this = this;
        if (typeof options == 'string') {
            return $(_this)._selectpicker(options, param);
        }
        var defaults = {
            value:[],
            url: null,
            data: [],
            valueField: 'value',
            textField: 'text',
        };

        var _options = $.extend(defaults, options);
        if (_options.url) {
            $.postJSON('/Base_SysManage/Base_SysRole/GetDataList_NoPagin', {}, function (resJson) {
                _options.data = resJson;
                build();
            });
        } else {
            build();
        }

        function build() {
            var data = _options.data;
            for (var i = 0; i < data.length; i++) {
                $(_this).append("<option value=" + data[i][_options.valueField] + ">" + data[i][_options.textField] + "</option>");
            }
            //重新渲染
            $(_this)._selectpicker('refresh');
            $(_this)._selectpicker('render');

            //赋初值
            $(_this)._selectpicker('val', _options.value);

            return $(_this);
        }
    };
})($)
