/*
$('#CategoryId').zTreeSelect({
    url: '/ScmManage/Scm_ScmProduct/GetPlatformProductCategory?SourceType=1'
});

$('#CategoryId').zTreeSelect('setOption',{});
$('#CategoryId').zTreeSelect('reload');
 */
(function () {
    $.fn.zTreeSelect = function (options, param) {
        var _this = this;

        var _inputObj = null;
        var _zTreeObj = null;
        var _iconObj = null;
        var _treeContentObj = null

        var _selectId = $(_this)[0].id;
        var _inputId = _selectId + '_input';
        var _treeContentId = _selectId + '_treeContent';
        var _treeId = _selectId + '_tree';
        var _iconId = _selectId + '_icon';

        var methods = {
            reload: function () {
                return init();
            },
            setOption: function (newOption) {
                var oldOption = getOption();
                newOption = $.extend({}, oldOption, newOption);
                setOption(newOption);
            }
        };

        if (typeof options == 'string') {//执行方法
            return methods[options](param);
        } else {//初始化
            var defaults = {
                url: null,
                value: null,
                multiple: false,
                data: [],
                _firstLoad: true
            };

            var _option = $.extend({}, defaults, options);
            setOption(_option);

            init();
        }

        function init() {
            $(_this).css('display', 'none');

            var option = getOption();
            getData(function () {
                renderHtml();
                renderZTree();

                //设置默认选中数据
                if (option.value) {
                    selectItem(option.value);
                }

                option._firstLoad = false;
                setOption(option);
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
            //创建容器
            if (option._firstLoad) {
                var container = $('<div style="position:relative;width:100%"></div>');
                var children = $(_this).parent().children();
                $(_this).parent().append(container);
                container.append(children);
            }

            //select项渲染
            $(_this).empty();
            $.each(option.data, function (index, item) {
                var text = item.name;
                var value = item.id;

                $(_this).append("<option value=" + value + ">" + text + "</option>");
            });
            $(_this).val(null);

            var customClass = $(_this).attr('data-class') || 'form-control';

            //input显示框渲染
            if (option._firstLoad) {
                var inputHtml = '<input id="' + _inputId + '" type="text" readonly class="' + customClass + '"/>';
                $(_this).after($(inputHtml));
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
                var emptyIcon = $('<span class="form-control-feedback glyphicon glyphicon-remove" style="right:30px;cursor:pointer;pointer-events:auto" />');
                $(_inputObj).after(emptyIcon);
                var tmpSpan = $('<span class="' + customClass + '" style="display:none"></span>');
                emptyIcon.after(tmpSpan);
                var iconHtml = '<span id="' + _iconId + '" class="form-control-feedback glyphicon glyphicon-chevron-left" />'
                $(tmpSpan).after($(iconHtml));
                _iconObj = $('#' + _iconId);

                emptyIcon.click(function () {
                    emptyItem();
                });

                //zTree下拉渲染
                var width = $(_inputObj).outerWidth();
                var treeHtml = '<div id="' + _treeContentId + '" class="menuContent dropdown-menu" style="display:none; position: absolute;width:' + width + 'px;background-color:white">';
                treeHtml += '<ul id="' + _treeId + '" class="ztree" style="margin-top:0; width:100%;"></ul>'
                treeHtml += '</div>';
                $(treeHtml).appendTo('body');
                _treeContentObj = $('#' + _treeContentId);
                _zTreeObj = $('#' + _treeId);
            }
        }

        function getOption() {
            return $(_this).data('option');
        }

        function setOption(option) {
            $(_this).data('option', option);
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
                        var values = [];

                        nodes.sort(function compare(a, b) { return a.id - b.id; });
                        for (var i = 0, l = nodes.length; i < l; i++) {
                            v += nodes[i].name + ",";
                            values.push(nodes[i].id);
                        }
                        if (v.length > 0) v = v.substring(0, v.length - 1);
                        $('#' + _inputId).val(v);
                        $(_this).val(values);
                    }
                }
            };

            $.fn.zTree.init($('#' + _treeId), setting, option.data);
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

        function selectItem(value) {
            var zTree = $.fn.zTree.getZTreeObj(_treeId);
            var node = zTree.getNodeByParam("id", value);
            var name = node.name;
            node.checked = true;
            zTree.updateNode(node);
            $(_this).val(value);
            $(_inputObj).val(name);
        }

        function emptyItem() {
            $(_this).val(null);
            $(_inputObj).val(null);
        }
    };
})();