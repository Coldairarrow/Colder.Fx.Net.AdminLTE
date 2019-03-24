(function () {
    $.fn.zTreeSelect = function (options, params) {
        var _this = this;

        var defaults = {
            url: null,
            value: null,
            multiple: false,
            data: [],
        };
        var _inputObj = null;
        var _zTreeObj = null;
        var _iconObj = null;
        var _treeContentObj = null

        var _selectId = $(_this)[0].id;
        var _inputId = _selectId + '_input';
        var _treeContentId = _selectId + '_treeContent';
        var _treeId = _selectId + '_tree';
        var _iconId = _selectId + '_icon';

        var _option = $.extend({}, defaults, options);
        setOption(_option);
        init();
        var methods = {

        };

        function init() {
            var option = getOption();
            getData(function () {
                renderHtml();
                renderZTree();
            });
        }

        function getData(next) {
            var option = getOption();
            //远程获取数据
            if (option.url) {
                $.postJSON(option.url, {}, function (resJson) {
                    option.data = resJson;
                    setOption(option);
                    next();
                });
            } else {
                next();
            }
        }

        function renderHtml() {
            var option = getOption();

            //select项渲染
            $(_this).css('display', 'none');
            //$(_this).empty();
            $.each(option.data, function (index, item) {
                var text = item.name;
                var value = item.id;

                $(_this).append("<option value=" + value + ">" + text + "</option>");
            });

            //input显示框渲染
            var inputHtml = '<input id="' + _inputId + '" type="text" readonly class="form-control"/>';
            $(inputHtml).appendTo($(_this).parent());
            _inputObj = $('#' + _inputId);
            option._inputObj = $('#' + _inputId);
            $(_inputObj).click(function () {
                var show = $(_inputObj).data('show');
                if (show) {
                    hideMenu();
                } else {
                    showMenu();
                }
            });

            //图标
            var iconHtml = '<span id="' + _iconId + '" class="form-control-feedback glyphicon glyphicon-chevron-left" style="right:10px" />'
            $(iconHtml).appendTo($(_this).parent());
            _iconObj = $('#' + _iconId);

            //zTree下拉渲染
            var width = $(_inputObj).outerWidth();
            var treeHtml = '<div id="' + _treeContentId + '" class="menuContent dropdown-menu" style="display:none; position: absolute;width:' + width + 'px;background-color:white">';
            treeHtml += '<ul id="' + _treeId + '" class="ztree" style="margin-top:0; width:100%;"></ul>'
            treeHtml += '</div>';
            $(treeHtml).appendTo('body');
            _treeContentObj = $('#' + _treeContentId);
            _zTreeObj = $('#' + _treeId);
        }

        function renderZTree() {
            var option = getOption();

            var setting = {
                view: {
                    dblClickExpand: false
                },
                data: {
                    simpleData: {
                        enable: true
                    }
                },
                callback: {
                    beforeClick: function () { },
                    onClick: function (e, treeId, treeNode) {
                        var zTree = $.fn.zTree.getZTreeObj(_treeId),
                            nodes = zTree.getSelectedNodes(),
                            v = "";
                        nodes.sort(function compare(a, b) { return a.id - b.id; });
                        for (var i = 0, l = nodes.length; i < l; i++) {
                            v += nodes[i].name + ",";
                        }
                        if (v.length > 0) v = v.substring(0, v.length - 1);
                        _inputObj.attr("value", v);
                    }
                }
            };

            $.fn.zTree.init(_zTreeObj, setting, option.data);
        }

        function getOption() {
            return $(_this).data('option');
        }

        function setOption(option) {
            $(_this).data('option', option);
        }

        function showMenu() {
            $(_inputObj).data('show', true);
            $(_iconObj).removeClass('glyphicon-chevron-left');
            $(_iconObj).addClass('glyphicon-chevron-down');
            var treeContentOffset = $(_inputObj).offset();
            $(_treeContentObj).css({ left: treeContentOffset.left + "px", top: treeContentOffset.top + _inputObj.outerHeight() + "px" }).slideDown("fast");
        }

        function hideMenu() {
            $(_iconObj).removeClass('glyphicon-chevron-down');
            $(_iconObj).addClass('glyphicon-chevron-left');

            $(_inputObj).data('show', false);
            $(_treeContentObj).fadeOut("fast");
        }
    };
})();