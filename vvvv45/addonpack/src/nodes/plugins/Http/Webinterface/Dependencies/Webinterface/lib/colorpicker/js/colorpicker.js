/**
 *
 * Color picker
 * Author: Stefan Petre www.eyecon.ro
 * 
 * MESO adjustments (seeger@meso.net) 
 *   - float 
 *   - HSB<->RGB
 * 
 */
resolution = 1.0;   // range of rgb values (default = 255 = 8 bit)
fltEps = 1.0e-5;

fltEq = function(x, y) {
  return (Math.abs(parseFloat(x) - parseFloat(y)) <= fltEps);
};

(function ($) {
	var ColorPicker = function () {
		var
			ids = {},
			inAction,
			charMin = 65,
			visible,
			tpl = '<div class="colorpicker"><div class="colorpicker_color"><div><div></div></div></div><div class="colorpicker_hue"><div></div></div><div class="colorpicker_new_color"></div><div class="colorpicker_current_color"></div><div class="colorpicker_hex"><input type="text" maxlength="6" /></div><div class="colorpicker_rgb_r colorpicker_rgb colorpicker_field"><input type="text" /><span></span></div><div class="colorpicker_rgb_g colorpicker_rgb colorpicker_field"><input type="text" /><span></span></div><div class="colorpicker_rgb_b colorpicker_rgb colorpicker_field"><input type="text" /><span></span></div><div class="colorpicker_hsb_h colorpicker_hsb colorpicker_field"><input type="text" /><span></span></div><div class="colorpicker_hsb_s colorpicker_hsb colorpicker_field"><input type="text" /><span></span></div><div class="colorpicker_hsb_b colorpicker_hsb colorpicker_field"><input type="text" /><span></span></div><div class="colorpicker_submit"></div></div>',
			defaults = {
				eventName: 'click',
				onShow: function () {},
				onBeforeShow: function(){},
				onHide: function () {},
				onChange: function () {},
				onChangeComplete: function () {},
				onSubmit: function () {},
				color: 'ff0000',
				livePreview: true,
				flat: false
			},
			fillRGBFields = function  (hsb, cal) {
				var rgb = fixRGB(HSBToRGB(hsb));
				$(cal).data('colorpicker').fields
					.eq(1).val(rgb.r).end()
					.eq(2).val(rgb.g).end()
					.eq(3).val(rgb.b).end();
			},
			fillHSBFields = function  (hsb, cal) {
        hsb = fixHSB(hsb)
				$(cal).data('colorpicker').fields
					.eq(4).val(hsb.h).end()
					.eq(5).val(hsb.s).end()
					.eq(6).val(hsb.b).end();
			},
			fillHexFields = function (hsb, cal) {
				$(cal).data('colorpicker').fields
					.eq(0).val(HSBToHex(hsb)).end();
			},
			setSelector = function (hsb, cal) {
				$(cal).data('colorpicker').selector.css('backgroundColor', '#' + HSBToHex({h: hsb.h, s: resolution, b: resolution}));
				$(cal).data('colorpicker').selectorIndic.css({
					left: parseInt(150.0 * hsb.s/resolution, 10),
					top: parseInt(150.0 * (resolution-hsb.b)/resolution, 10)
				});
			},
			setHue = function (hsb, cal) {
				$(cal).data('colorpicker').hue.css('top', parseInt(150.0 - 150.0 * hsb.h/360.0, 10));
			},
			setCurrentColor = function (hsb, cal) {
				$(cal).data('colorpicker').currentColor.css('backgroundColor', '#' + HSBToHex(hsb));
			},
			setNewColor = function (hsb, cal) {
				$(cal).data('colorpicker').newColor.css('backgroundColor', '#' + HSBToHex(hsb));
			},
			keyDown = function (ev) {
				var pressedKey = ev.charCode || ev.keyCode || -1;
				if ((pressedKey > charMin && pressedKey <= 90) || pressedKey == 32) {
					return false;
				}
				var cal = $(this).parent().parent();
				if (cal.data('colorpicker').livePreview === true) {
					change.apply(this);
				}
			},
			change = function (ev) {
				var cal = $(this).parent().parent(), col;
				if (this.parentNode.className.indexOf('_hex') > 0) {
					cal.data('colorpicker').color = col = fixHSB(HexToHSB(fixHex(this.value)));
//          alert(col.h + ", " + col.s + ", " + col.b);
				} else if (this.parentNode.className.indexOf('_hsb') > 0) {
					cal.data('colorpicker').color = col = fixHSB({
						h: parseFloat(cal.data('colorpicker').fields.eq(4).val(), 10),
						s: parseFloat(cal.data('colorpicker').fields.eq(5).val(), 10),
						b: parseFloat(cal.data('colorpicker').fields.eq(6).val(), 10)
					});
				} else {
					cal.data('colorpicker').color = col = RGBToHSB({
						r: parseFloat(cal.data('colorpicker').fields.eq(1).val(), 10),
						g: parseFloat(cal.data('colorpicker').fields.eq(2).val(), 10),
						b: parseFloat(cal.data('colorpicker').fields.eq(3).val(), 10)
					});
				}
				if (ev) {
					fillRGBFields(col, cal.get(0));
					fillHexFields(col, cal.get(0));
					fillHSBFields(col, cal.get(0));
				}
				setSelector(col, cal.get(0));
				setHue(col, cal.get(0));
				setNewColor(col, cal.get(0));
				cal.data('colorpicker').onChange.apply(cal, [col, HSBToHex(col), HSBToRGB(col)]);
			},
			blur = function (ev) {
				var cal = $(this).parent().parent();
				cal.data('colorpicker').fields.parent().removeClass('colorpicker_focus')
			},
			focus = function () {
				charMin = this.parentNode.className.indexOf('_hex') > 0 ? 70 : 65;
				$(this).parent().parent().data('colorpicker').fields.parent().removeClass('colorpicker_focus');
				$(this).parent().addClass('colorpicker_focus');
			},
			downIncrement = function (ev) {
				var field = $(this).parent().find('input').focus();
				var current = {
					cal: $(this).parent().parent(),
					el: $(this).parent().addClass('colorpicker_slider'),
					max: this.parentNode.className.indexOf('_hsb_h') > 0 ? 360.0 : (this.parentNode.className.indexOf('_hsb') > 0 ? resolution : resolution),
					y: ev.pageY,
					field: field,
					scale: this.parentNode.className.indexOf('_hsb_h') <= 0 ? resolution / 400.0 : 0.5,
					val: parseFloat(field.val(), 10),
					preview: $(this).parent().parent().data('colorpicker').livePreview					
				};
				$(document).bind('mouseup', current, upIncrement);
				$(document).bind('mousemove', current, moveIncrement);
			},
			moveIncrement = function (ev) {
				ev.data.field.val(Math.max(0, Math.min(ev.data.max, parseFloat((ev.data.val + (ev.data.y - ev.pageY) * ev.data.scale), 10) )));
				if (ev.data.preview) {
					change.apply(ev.data.field.get(0), [true]);
				}
				return false;
			},
			upIncrement = function (ev) {
				change.apply(ev.data.field.get(0), [true]);
				ev.data.el.removeClass('colorpicker_slider').find('input').focus();
				$(document).unbind('mouseup', upIncrement);
				$(document).unbind('mousemove', moveIncrement);
				var cal = ev.data.cal;
				var col = cal.data('colorpicker').color;
				cal.data('colorpicker').onChangeComplete.apply(cal, [col, HSBToHex(col), HSBToRGB(col)]);
				return false;
			},
			downHue = function (ev) {
				var current = {
					cal: $(this).parent(),
					y: $(this).offset().top
				};
				current.preview = current.cal.data('colorpicker').livePreview;
				$(document).bind('mouseup', current, upHue);
				$(document).bind('mousemove', current, moveHue);
			},
			moveHue = function (ev) {
				change.apply(
					ev.data.cal.data('colorpicker')
						.fields
						.eq(4)
						.val(parseFloat(360.0*(150.0 - Math.max(0.0,Math.min(150.0,(ev.pageY - ev.data.y))))/150.0, 10))
						.get(0),
					[ev.data.preview]
				);
				return false;
			},
			upHue = function (ev) {
				var cal = ev.data.cal;
				var col = cal.data('colorpicker').color;
				fillRGBFields(col, cal.get(0));
				fillHexFields(col, cal.get(0));
				$(document).unbind('mouseup', upHue);
				$(document).unbind('mousemove', moveHue);
				cal.data('colorpicker').onChangeComplete.apply(cal, [col, HSBToHex(col), HSBToRGB(col)]);
				return false;
			},
			downSelector = function (ev) {
				var current = {
					cal: $(this).parent(),
					pos: $(this).offset()
				};
				current.preview = current.cal.data('colorpicker').livePreview;
				$(document).bind('mouseup', current, upSelector);
				$(document).bind('mousemove', current, moveSelector);
			},
			moveSelector = function (ev) {
				change.apply(
					ev.data.cal.data('colorpicker')
						.fields
						.eq(6)
						.val(parseFloat(resolution*(150.0 - Math.max(0.0,Math.min(150.0,(ev.pageY - ev.data.pos.top))))/150.0, 10))
						.end()
						.eq(5)
						.val(parseFloat(resolution*(Math.max(0,Math.min(150.0,(ev.pageX - ev.data.pos.left))))/150.0, 10))
						.get(0),
					[ev.data.preview]
				);
				return false;
			},
			upSelector = function (ev) {
				var cal = ev.data.cal;
				var col = cal.data('colorpicker').color;
				fillRGBFields(col, cal.get(0));
				fillHexFields(col, cal.get(0));
				$(document).unbind('mouseup', upSelector);
				$(document).unbind('mousemove', moveSelector);
				cal.data('colorpicker').onChangeComplete.apply(cal, [col, HSBToHex(col), HSBToRGB(col)]);
				return false;
			},
			enterSubmit = function (ev) {
				$(this).addClass('colorpicker_focus');
			},
			leaveSubmit = function (ev) {
				$(this).removeClass('colorpicker_focus');
			},
			clickSubmit = function (ev) {
				var cal = $(this).parent();
				var col = cal.data('colorpicker').color;
				cal.data('colorpicker').origColor = col;
				setCurrentColor(col, cal.get(0));
				cal.data('colorpicker').onSubmit(col, HSBToHex(col), HSBToRGB(col));
			},
			show = function (ev) {
				var cal = $('#' + $(this).data('colorpickerId'));
				cal.data('colorpicker').onBeforeShow.apply(this, [cal.get(0)]);
				var pos = $(this).offset();
				var viewPort = getViewport();
				var top = pos.top + this.offsetHeight;
				var left = pos.left;
				if (top + 176 > viewPort.t + viewPort.h) {
					top -= this.offsetHeight + 176;
				}
				if (left + 356.0 > viewPort.l + viewPort.w) {
					left -= 356.0;
				}
				cal.css({left: left + 'px', top: top + 'px'});
				if (cal.data('colorpicker').onShow.apply(this, [cal.get(0)]) != false) {
					cal.show();
				}
				$(document).bind('mousedown', {cal: cal}, hide);
				return false;
			},
			hide = function (ev) {
				if (!isChildOf(ev.data.cal.get(0), ev.target, ev.data.cal.get(0))) {
					if (ev.data.cal.data('colorpicker').onHide.apply(this, [ev.data.cal.get(0)]) != false) {
						ev.data.cal.hide();
					}
					$(document).unbind('mousedown', hide);
				}
			},
			isChildOf = function(parentEl, el, container) {
				if (parentEl == el) {
					return true;
				}
				if (parentEl.contains) {
					return parentEl.contains(el);
				}
				if ( parentEl.compareDocumentPosition ) {
					return !!(parentEl.compareDocumentPosition(el) & 16);
				}
				var prEl = el.parentNode;
				while(prEl && prEl != container) {
					if (prEl == parentEl)
						return true;
					prEl = prEl.parentNode;
				}
				return false;
			},
			getViewport = function () {
				var m = document.compatMode == 'CSS1Compat';
				return {
					l : window.pageXOffset || (m ? document.documentElement.scrollLeft : document.body.scrollLeft),
					t : window.pageYOffset || (m ? document.documentElement.scrollTop : document.body.scrollTop),
					w : window.innerWidth || (m ? document.documentElement.clientWidth : document.body.clientWidth),
					h : window.innerHeight || (m ? document.documentElement.clientHeight : document.body.clientHeight)
				};
			},
			fixHSB = function (hsb) {
				return {
					h: Math.min(360.0, Math.max(0.0, hsb.h)),
					s: Math.min(resolution, Math.max(0.0, hsb.s)),
					b: Math.min(resolution, Math.max(0.0, hsb.b))
				};
			}, 
			fixRGB = function (rgb) {
				return {
					r: Math.min(resolution, Math.max(0.0, rgb.r)),
					g: Math.min(resolution, Math.max(0.0, rgb.g)),
					b: Math.min(resolution, Math.max(0.0, rgb.b))
				};
			},
			fixHex = function (hex) {
				var len = 6 - hex.length;
				if (len > 0) {
					var o = [];
					for (var i=0; i<len; i++) {
						o.push('0');
					}
					o.push(hex);
					hex = o.join('');
				}
				return hex;
			}, 
			HexToRGB = function (hex) {
				var hex = parseInt(((hex.indexOf('#') > -1) ? hex.substring(1) : hex), 16);
				return {r: (hex >> 16) * resolution / 255.0, 
                g: ((hex & 0x00FF00) >> 8) * resolution / 255.0,
                b: (hex & 0x0000FF) * resolution / 255.0};
			},
			HexToHSB = function (hex) {
				return RGBToHSB(HexToRGB(hex));
			},
			RGBToHSB = function (rgb) {
//        var hsb = {};
/*        
 * 
 *  **** CLEANER ALGORITHM. STILL BUGGY, THOUGH; IMPLEMENT LATER? 
 * 
 * 
 
        //rgb = fixRGB(rgb);
        
        // norm + determine min/max
        var r = parseFloat(rgb.r / resolution); var g = parseFloat(rgb.g / resolution); var b = parseFloat(rgb.b / resolution);
        var max = parseFloat(Math.max(r, Math.max(g, b))); var min = parseFloat(Math.min(r, Math.min(g, b)));
                
        // H
        if (fltEq(r, g) && fltEq(r, b)) {  // r = g = b
          hsb.h = 0.0;
        }
        else if((r >= g) && (r >= b)) {  // max = r
          hsb.h = 60.0 * (0.0 + (g - b) / (max - min));
        }
        else if((g >= r) && (g >= b)) {  // max = g
          hsb.h = 60.0 * (2.0 + (b - r) / (max - min));
        }
        else if((b >= r) && (b >= g)) {  // max = b
          hsb.h = 60.0 * (4.0 + (r - g) / (max - min));
        }

        // S
        if (fltEq(max, 0.0)) {
          hsb.s = 0.0;
        }
        else {
          hsb.s = (max - min) / max;
        }
        
        // B
        hsb.b = max;

        hsb.s *= 100.0; hsb.b *= 100.0;
        //hsb = fixHSB(hsb);
        
        return hsb;
*/
//        alert("r: " + rgb.r + ", g: " + rgb.g + ", b: " + rgb.b);

        //var rgb = {r: rgb_orig / resolution, g: rgb_or} 

				var hsb = {};
				hsb.b = Math.max(Math.max(rgb.r,rgb.g),rgb.b);
				//hsb.s = (hsb.b <= 0) ? 0.0 : Math.round(100.0*(hsb.b - Math.min(Math.min(rgb.r,rgb.g),rgb.b))/hsb.b);
				hsb.s = (hsb.b <= 0) ? 0.0 : resolution*(hsb.b - Math.min(Math.min(rgb.r,rgb.g),rgb.b))/hsb.b;
				hsb.b = (hsb.b / resolution)*resolution; //Math.round((hsb.b / resolution)*100.0);
				if((rgb.r==rgb.g) && (rgb.g==rgb.b)) hsb.h = 0.0;
				else if(rgb.r>=rgb.g && rgb.g>=rgb.b) hsb.h = 60.0*(rgb.g-rgb.b)/(rgb.r-rgb.b);
				else if(rgb.g>=rgb.r && rgb.r>=rgb.b) hsb.h = 60.0  + 60.0*(rgb.g-rgb.r)/(rgb.g-rgb.b);
				else if(rgb.g>=rgb.b && rgb.b>=rgb.r) hsb.h = 120.0 + 60.0*(rgb.b-rgb.r)/(rgb.g-rgb.r);
				else if(rgb.b>=rgb.g && rgb.g>=rgb.r) hsb.h = 180.0 + 60.0*(rgb.b-rgb.g)/(rgb.b-rgb.r);
				else if(rgb.b>=rgb.r && rgb.r>=rgb.g) hsb.h = 240.0 + 60.0*(rgb.r-rgb.g)/(rgb.b-rgb.g);
				else if(rgb.r>=rgb.b && rgb.b>=rgb.g) hsb.h = 300.0 + 60.0*(rgb.r-rgb.b)/(rgb.r-rgb.g);
				else hsb.h = 0;
				//hsb.h = Math.round(hsb.h);
        
        hsb.h = parseFloat(Math.round(hsb.h * 1000.0)) / 1000.0;
        hsb.s = parseFloat(Math.round(hsb.s * 1000.0)) / 1000.0;
        hsb.b = parseFloat(Math.round(hsb.b * 1000.0)) / 1000.0;
        
				return hsb;

			},
			HSBToRGB = function (hsb) {
//        var rgb = {};
        
        //hsb = fixHSB(hsb);
  /*      
/*        
 * 
 *  **** CLEANER ALGORITHM. STILL BUGGY, THOUGH; IMPLEMENT LATER? 
 *
 * 
        var h = parseFloat(hsb.h); var s = parseFloat(hsb.s) / 100.0; var b = parseFloat(hsb.b) / 100.0;
        var hi = parseInt(Math.round(h / 60.0));
        var f = h / 60.0 - parseFloat(hi);
        var p = b * (1.0 - s);
        var q = b * (1.0 - s * f);
        var t = b * (1.0 - s * (1.0 - f));

        //alert("hsb: (" + h + ", " + s + ", " + b + ")\n");
        
        switch(hi) {
          case 0: 
          case 6: rgb.r = b; rgb.g = t; rgb.b = p;
                  break;
          case 1: rgb.r = q; rgb.g = b; rgb.b = p;
                  break;
          case 2: rgb.r = p; rgb.g = b; rgb.b = t;
                  break;
          case 3: rgb.r = p; rgb.g = q; rgb.b = b;
                  break;
          case 4: rgb.r = t; rgb.g = p; rgb.b = b;
                  break;
          case 5: rgb.r = b; rgb.g = p; rgb.b = q;
                  break;
        }

        rgb.r *= resolution; rgb.g *= resolution; rgb.b *= resolution;
          
        //rgb = fixRGB(rgb);
        
        return rgb;
*/
				var rgb = {};
				var h = hsb.h; //parseInt(Math.round(hsb.h));
				var s = hsb.s*resolution/resolution; //Math.round(hsb.s*resolution/100.0);
				var v = hsb.b*resolution/resolution; //Math.round(hsb.b*resolution/100.0);
				if(s == 0) {
					rgb.r = rgb.g = rgb.b = v;
				} else {
					var t1 = v;
					var t2 = (resolution-s)*v/resolution;
					var t3 = (t1-t2)*(parseInt(Math.round(h))%60)/60.0;
					if(parseInt(Math.round(h))==360) h = 0.0;
					if(h<60.0) {rgb.r=t1;	rgb.b=t2; rgb.g=t2+t3}
					else if(h<120.0) {rgb.g=t1; rgb.b=t2;	rgb.r=t1-t3}
					else if(h<180.0) {rgb.g=t1; rgb.r=t2;	rgb.b=t2+t3}
					else if(h<240.0) {rgb.b=t1; rgb.r=t2;	rgb.g=t1-t3}
					else if(h<300.0) {rgb.b=t1; rgb.g=t2;	rgb.r=t2+t3}
					else if(h<360.0) {rgb.r=t1; rgb.g=t2;	rgb.b=t1-t3}
					else {rgb.r=0.0; rgb.g=0.0;	rgb.b=0.0}
				}
				//return {r:Math.round(rgb.r), g:Math.round(rgb.g), b:Math.round(rgb.b)};

        rgb.r = parseFloat(Math.round(rgb.r * 1000.0)) / 1000.0;
        rgb.g = parseFloat(Math.round(rgb.g * 1000.0)) / 1000.0;
        rgb.b = parseFloat(Math.round(rgb.b * 1000.0)) / 1000.0;

        return rgb;

			},
			RGBToHex = function (rgb) {
				var hex = [
					parseInt(rgb.r / resolution * 255.0, 10).toString(16),
					parseInt(rgb.g / resolution * 255.0, 10).toString(16),
					parseInt(rgb.b / resolution * 255.0, 10).toString(16),
				];
				$.each(hex, function (nr, val) {
					if ((val != undefined) && (val.length == 1)) {
						hex[nr] = '0' + val;
					}
				});
				return hex.join('');
			},
			HSBToHex = function (hsb) {
				return RGBToHex(HSBToRGB(hsb));
			};
		return {
			init: function (options) {
				options = $.extend({}, defaults, options||{});
				if (typeof options.color == 'string') {
					options.color = HexToHSB(options.color);
				} else if (options.color.r != undefined && options.color.g != undefined && options.color.b != undefined) {
					options.color = RGBToHSB(options.color);
				} else if (options.color.h != undefined && options.color.s != undefined && options.color.b != undefined) {
					options.color = fixHSB(options.color);
				} else {
					return this;
				}
				options.origColor = options.color;
				return this.each(function () {
					if (!$(this).data('colorpickerId')) {
						var id = 'colorpicker_' + parseInt(Math.random() * 1000);
						$(this).data('colorpickerId', id);
						var cal = $(tpl).attr('id', id);
						if (options.flat) {
							cal.appendTo(this).show();
						} else {
							cal.appendTo(document.body);
						}
						options.fields = cal
											.find('input')
												.bind('keydown', keyDown)
												.bind('change', change)
												.bind('blur', blur)
												.bind('focus', focus);
						cal.find('span').bind('mousedown', downIncrement);
						options.selector = cal.find('div.colorpicker_color').bind('mousedown', downSelector);
						options.selectorIndic = options.selector.find('div div');
						options.hue = cal.find('div.colorpicker_hue div');
						cal.find('div.colorpicker_hue').bind('mousedown', downHue);
						options.newColor = cal.find('div.colorpicker_new_color');
						options.currentColor = cal.find('div.colorpicker_current_color');
						cal.data('colorpicker', options);
						cal.find('div.colorpicker_submit')
							.bind('mouseenter', enterSubmit)
							.bind('mouseleave', leaveSubmit)
							.bind('click', clickSubmit);
						fillRGBFields(options.color, cal.get(0));
						fillHSBFields(options.color, cal.get(0));
						fillHexFields(options.color, cal.get(0));
						setHue(options.color, cal.get(0));
						setSelector(options.color, cal.get(0));
						setCurrentColor(options.color, cal.get(0));
						setNewColor(options.color, cal.get(0));
						if (options.flat) {
							cal.css({
								position: 'relative',
								display: 'block'
							});
						} else {
							$(this).bind(options.eventName, show);
						}
					}
				});
			},
			showPicker: function() {
				return this.each( function () {
					if ($(this).data('colorpickerId')) {
						show.apply(this);
					}
				});
			},
			hidePicker: function() {
				return this.each( function () {
					if ($(this).data('colorpickerId')) {
						$('#' + $(this).data('colorpickerId')).hide();
					}
				});
			},
			setColor: function(col) {
				if (typeof col == 'string') {
					col = HexToHSB(col);
				} else if (col.r != undefined && col.g != undefined && col.b != undefined) {
					col = RGBToHSB(col);
				} else if (col.h != undefined && col.s != undefined && col.b != undefined) {
					col = fixHSB(col);
				} else {
					return this;
				}
				return this.each(function(){
					if ($(this).data('colorpickerId')) {
						var cal = $('#' + $(this).data('colorpickerId'));
						cal.data('colorpicker').color = col;
						cal.data('colorpicker').origColor = col;
						fillRGBFields(col, cal.get(0));
						fillHSBFields(col, cal.get(0));
						fillHexFields(col, cal.get(0));
						setHue(col, cal.get(0));
						setSelector(col, cal.get(0));
						setCurrentColor(col, cal.get(0));
						setNewColor(col, cal.get(0));
					}
				});
			}
		};
	}();
	$.fn.extend({
		ColorPicker: ColorPicker.init,
		ColorPickerHide: ColorPicker.hide,
		ColorPickerShow: ColorPicker.show,
		ColorPickerSetColor: ColorPicker.setColor
	});
})(jQuery)
