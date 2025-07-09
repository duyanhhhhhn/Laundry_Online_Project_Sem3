$(document).ready(function ($) {
	awe_backtotop();
	awe_category();
	$('#trigger-mobile').click(function(){
		$(".mobile-main-menu").toggleClass('active');
		$(".backdrop__body-backdrop___1rvky").addClass('active');
	});
	$('.evo-header-cart').click(function(){
		$(".cart_sidebar").toggleClass('active');
		$(".backdrop__body-backdrop___1rvky").addClass('active');
	});
	$('.header-search').click(function(){
		$(".evo_sidebar_search").toggleClass('active');
		$(".backdrop__body-backdrop___1rvky").addClass('active');
	});
	$('.backdrop__body-backdrop___1rvky, .evo-close-menu, .cart_btn-close, .search_close').click(function(){
		$("body").removeClass('show-search');
		$(".mobile-main-menu, .cart_sidebar, .evo_sidebar_search, .left-content").removeClass('active');
		$(".backdrop__body-backdrop___1rvky").removeClass('active');
	});
	$(".backdrop__body-backdrop___1rvky").removeClass('active');
	$('.ng-has-child1 a .svg1').on('click', function(e){
		e.preventDefault();var $this = $(this);
		$this.parents('.ng-has-child1').find('.ul-has-child1').stop().slideToggle();
		$(this).toggleClass('active');
		return false;
	});
	$('.ng-has-child1 .ul-has-child1 .ng-has-child2 a .svg2').on('click', function(e){
		e.preventDefault();var $this = $(this);
		$this.parents('.ng-has-child1 .ul-has-child1 .ng-has-child2').find('.ul-has-child2').stop().slideToggle();
		$(this).toggleClass('active');
		return false;
	});
	if($('.cart_body>div').length == '0' ){
		$('.cart-footer').hide();
		jQuery('<div class="cart-empty">'
			   + '<span class="empty-icon"><i class="ico ico-cart"></i></span>'
			   + '<div class="btn-cart-empty">'
			   + '<a class="btn btn-default" href="/" title="Tiếp tục mua hàng">Tiếp tục mua hàng</a>'
			   + '</div>'
			   + '</div>').appendTo('.cart_body');
	};
});
$(document).on('click','.overlay, .close-popup, .btn-continue, .fancybox-close', function() {   
	hidePopup('.awe-popup'); 	
	setTimeout(function(){$('.loading').removeClass('loaded-content');},500);
	return false;
})
function awe_category(){
	$('.nav-category .Collapsible__Plus').click(function(e){
		$(this).parent().toggleClass('active');
	});
} window.awe_category=awe_category;
function awe_showNoitice(selector) {
	$(selector).animate({right: '0'}, 500);
	setTimeout(function(){$(selector).animate({right: '-300px'}, 500);}, 3500);
}  window.awe_showNoitice=awe_showNoitice;
function awe_showLoading(selector) {
	var loading = $('.loader').html();
	$(selector).addClass("loading").append(loading); 
}  window.awe_showLoading=awe_showLoading;
function awe_hideLoading(selector) {
	$(selector).removeClass("loading"); 
	$(selector + ' .loading-icon').remove();
}  window.awe_hideLoading=awe_hideLoading;
function awe_showPopup(selector) {
	$(selector).addClass('active');
}  window.awe_showPopup=awe_showPopup;
function awe_hidePopup(selector) {
	$(selector).removeClass('active');
}  window.awe_hidePopup=awe_hidePopup;
function awe_convertVietnamese(str) { 
	str= str.toLowerCase();str= str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g,"a");str= str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g,"e");str= str.replace(/ì|í|ị|ỉ|ĩ/g,"i");str= str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g,"o"); str= str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g,"u");str= str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g,"y");str= str.replace(/đ/g,"d"); str= str.replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'| |\"|\&|\#|\[|\]|~|$|_/g,"-");str= str.replace(/-+-/g,"-");str= str.replace(/^\-+|\-+$/g,""); 
	return str; 
} window.awe_convertVietnamese=awe_convertVietnamese;
function awe_backtotop() { 
	if ($('.back-to-top').length) {
		var scrollTrigger = 100,
			backToTop = function () {
				var scrollTop = $(window).scrollTop();
				if (scrollTop > scrollTrigger) {
					$('.back-to-top').addClass('show');
				} else {
					$('.back-to-top').removeClass('show');
				}
			};
		backToTop();
		$(window).on('scroll', function () {
			backToTop();
		});
		$('.back-to-top').on('click', function (e) {
			e.preventDefault();
			$('html,body').animate({
				scrollTop: 0
			}, 700);
		});
	}
} window.awe_backtotop=awe_backtotop;
$('.btn-close').click(function() {
	$(this).parents('.dropdown').toggleClass('open');
}); 
$(document).on('keydown','#qty, #quantity-detail, .number-sidebar, .number-phone',function(e){-1!==$.inArray(e.keyCode,[46,8,9,27,13,110,190])||/65|67|86|88/.test(e.keyCode)&&(!0===e.ctrlKey||!0===e.metaKey)||35<=e.keyCode&&40>=e.keyCode||(e.shiftKey||48>e.keyCode||57<e.keyCode)&&(96>e.keyCode||105<e.keyCode)&&e.preventDefault()});
var buy_now = function(id) {
	var quantity = 1;
	var params = {
		type: 'POST',
		url: '/cart/add.js',
		data: 'quantity=' + quantity + '&variantId=' + id,
		dataType: 'json',
		success: function(line_item) {
			window.location = '/checkout';
		},
		error: function(XMLHttpRequest, textStatus) {
			Bizweb.onError(XMLHttpRequest, textStatus);
		}
	};
	jQuery.ajax(params);
}
var _0xa1c3=["\x74\x68\x65\x6D\x65"];window[_0xa1c3[0]]= window[_0xa1c3[0]]|| {}
theme.wishlist = (function (){
	var wishlistButtonClass = '.js-btn-wishlist',
		wishlistRemoveButtonClass = '.js-remove-wishlist',
		$wishlistCount = $('.js-wishlist-count'),
		$wishlistContainer = $('.js-wishlist-content'),
		$wishlistSmall = $('.wish-list-small'),
		wishlistViewAll = $('.wish-list-button-all'),
		wishlistObject = JSON.parse(localStorage.getItem('localWishlist')) || [],
		wishlistPageUrl = $('.js-wishlist-link').attr('href'),
		loadNoResult = function (){
			$wishlistContainer.html('<div class="col text-center"><div class="alert alert-warning d-inline-block"><h3>Sản phẩm nào của chúng tôi bạn mong muốn sở hữu nhất?</h3><p>Hãy thêm vào danh sách sản phẩm yêu thích ngay nhé!</p></div></div>');
			$wishlistSmall.html('<div class="empty-description"><span class="empty-icon"><i class="ico ico-favorite-heart"></i></span><div class="empty-text"><h3>Sản phẩm nào của chúng tôi bạn mong muốn sở hữu nhất?</h3><p>Hãy thêm vào danh sách sản phẩm yêu thích ngay nhé!</p></div></div><style>.container--wishlist .js-wishlist-content{border:none;}</style>');
			wishlistViewAll.addClass('d-none');
		};
	function loadWishlist(){
		$wishlistContainer.html('');
		if (wishlistObject.length > 0){
			var recentview_wishlist = [];
			for (var i = 0; i < wishlistObject.length; i++) { 
				var productHandle = wishlistObject[i];
				for (var i = 0; i < wishlistObject.length; i++) { 
					var productHandle = wishlistObject[i];
					var wishlist = new Promise(function(resolve, reject) {
						$.ajax({
							url:'/' + productHandle + '?view=wishlist',
							success: function(product){
								resolve(product);
							},
							error: function(err){
								resolve('');
							}
						})
					});
					recentview_wishlist.push(wishlist);
				}
				Promise.all(recentview_wishlist).then(function(values) {
					$.each(values, function(i, v){
						$('.js-wishlist-content').append(v);
					});
					awe_lazyloadImage();
				});
			}
		}else{
			loadNoResult();
		}
		$wishlistCount.text(wishlistObject.length);
		$(wishlistButtonClass).each(function(){
			var productHandle = $(this).data('handle');
			var iconWishlist = $.inArray(productHandle,wishlistObject) !== -1 ? "<svg class='evo-added' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 51.997 51.997' style='enable-background:new 0 0 51.997 51.997;' xml:space='preserve'><path d='M51.911,16.242C51.152,7.888,45.239,1.827,37.839,1.827c-4.93,0-9.444,2.653-11.984,6.905c-2.517-4.307-6.846-6.906-11.697-6.906c-7.399,0-13.313,6.061-14.071,14.415c-0.06,0.369-0.306,2.311,0.442,5.478c1.078,4.568,3.568,8.723,7.199,12.013l18.115,16.439l18.426-16.438c3.631-3.291,6.121-7.445,7.199-12.014C52.216,18.553,51.97,16.611,51.911,16.242z M49.521,21.261c-0.984,4.172-3.265,7.973-6.59,10.985L25.855,47.481L9.072,32.25c-3.331-3.018-5.611-6.818-6.596-10.99c-0.708-2.997-0.417-4.69-0.416-4.701l0.015-0.101C2.725,9.139,7.806,3.826,14.158,3.826c4.687,0,8.813,2.88,10.771,7.515l0.921,2.183l0.921-2.183c1.927-4.564,6.271-7.514,11.069-7.514c6.351,0,11.433,5.313,12.096,12.727C49.938,16.57,50.229,18.264,49.521,21.261z'/></svg>" : "<svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 51.997 51.997' style='enable-background:new 0 0 51.997 51.997;' xml:space='preserve'><path d='M51.911,16.242C51.152,7.888,45.239,1.827,37.839,1.827c-4.93,0-9.444,2.653-11.984,6.905c-2.517-4.307-6.846-6.906-11.697-6.906c-7.399,0-13.313,6.061-14.071,14.415c-0.06,0.369-0.306,2.311,0.442,5.478c1.078,4.568,3.568,8.723,7.199,12.013l18.115,16.439l18.426-16.438c3.631-3.291,6.121-7.445,7.199-12.014C52.216,18.553,51.97,16.611,51.911,16.242z M49.521,21.261c-0.984,4.172-3.265,7.973-6.59,10.985L25.855,47.481L9.072,32.25c-3.331-3.018-5.611-6.818-6.596-10.99c-0.708-2.997-0.417-4.69-0.416-4.701l0.015-0.101C2.725,9.139,7.806,3.826,14.158,3.826c4.687,0,8.813,2.88,10.771,7.515l0.921,2.183l0.921-2.183c1.927-4.564,6.271-7.514,11.069-7.514c6.351,0,11.433,5.313,12.096,12.727C49.938,16.57,50.229,18.264,49.521,21.261z'/></svg>";
			var textWishlist = $.inArray(productHandle,wishlistObject) !== -1 ? "Đến trang sản phẩm yêu thích" : "Thêm vào yêu thích";
			$(this).html(iconWishlist).attr('title',textWishlist);
		});
	}
	var _0xcd91=["\x68\x61\x6E\x64\x6C\x65","\x64\x61\x74\x61","\x5B\x64\x61\x74\x61\x2D\x68\x61\x6E\x64\x6C\x65\x3D\x22","\x22\x5D","\x69\x6E\x41\x72\x72\x61\x79","\x68\x72\x65\x66","\x6C\x6F\x63\x61\x74\x69\x6F\x6E","\x70\x75\x73\x68","\x77\x69\x73\x68\x6C\x69\x73\x74\x49\x63\x6F\x6E\x41\x64\x64\x65\x64","\x73\x74\x72\x69\x6E\x67\x73","\x68\x74\x6D\x6C","\x66\x61\x73\x74","\x66\x61\x64\x65\x49\x6E","\x73\x6C\x6F\x77","\x66\x61\x64\x65\x4F\x75\x74","\x74\x69\x74\x6C\x65","\x77\x69\x73\x68\x6C\x69\x73\x74\x54\x65\x78\x74\x41\x64\x64\x65\x64","\x61\x74\x74\x72","\x6C\x6F\x63\x61\x6C\x57\x69\x73\x68\x6C\x69\x73\x74","\x73\x74\x72\x69\x6E\x67\x69\x66\x79","\x73\x65\x74\x49\x74\x65\x6D","\x6C\x65\x6E\x67\x74\x68","\x74\x65\x78\x74"];function updateWishlist(_0xfc06x2){var _0xfc06x3=$(_0xfc06x2)[_0xcd91[1]](_0xcd91[0]),_0xfc06x4=$(wishlistButtonClass+ _0xcd91[2]+ _0xfc06x3+ _0xcd91[3]);var _0xfc06x5=$[_0xcd91[4]](_0xfc06x3,wishlistObject)!==  -1?true:false;if(_0xfc06x5){window[_0xcd91[6]][_0xcd91[5]]= wishlistPageUrl}else {wishlistObject[_0xcd91[7]](_0xfc06x3);_0xfc06x4[_0xcd91[14]](_0xcd91[13])[_0xcd91[12]](_0xcd91[11])[_0xcd91[10]](theme[_0xcd91[9]][_0xcd91[8]]);_0xfc06x4[_0xcd91[17]](_0xcd91[15],theme[_0xcd91[9]][_0xcd91[16]])};localStorage[_0xcd91[20]](_0xcd91[18],JSON[_0xcd91[19]](wishlistObject));$wishlistCount[_0xcd91[22]](wishlistObject[_0xcd91[21]])}
	var _0xd3ea=["\x63\x6C\x69\x63\x6B","\x70\x72\x65\x76\x65\x6E\x74\x44\x65\x66\x61\x75\x6C\x74","\x6F\x6E","\x68\x61\x6E\x64\x6C\x65","\x64\x61\x74\x61","\x5B\x64\x61\x74\x61\x2D\x68\x61\x6E\x64\x6C\x65\x3D\x22","\x22\x5D","\x77\x69\x73\x68\x6C\x69\x73\x74\x49\x63\x6F\x6E","\x73\x74\x72\x69\x6E\x67\x73","\x68\x74\x6D\x6C","\x74\x69\x74\x6C\x65","\x77\x69\x73\x68\x6C\x69\x73\x74\x54\x65\x78\x74","\x61\x74\x74\x72","\x69\x6E\x64\x65\x78\x4F\x66","\x73\x70\x6C\x69\x63\x65","\x6C\x6F\x63\x61\x6C\x57\x69\x73\x68\x6C\x69\x73\x74","\x73\x74\x72\x69\x6E\x67\x69\x66\x79","\x73\x65\x74\x49\x74\x65\x6D","\x66\x61\x64\x65\x4F\x75\x74","\x2E\x6A\x73\x2D\x77\x69\x73\x68\x6C\x69\x73\x74\x2D\x69\x74\x65\x6D","\x63\x6C\x6F\x73\x65\x73\x74","\x6C\x65\x6E\x67\x74\x68","\x74\x65\x78\x74"];$(document)[_0xd3ea[2]](_0xd3ea[0],wishlistButtonClass,function(_0xdfa5x1){_0xdfa5x1[_0xd3ea[1]]();updateWishlist(this)});$(document)[_0xd3ea[2]](_0xd3ea[0],wishlistRemoveButtonClass,function(){var _0xdfa5x2=$(this)[_0xd3ea[4]](_0xd3ea[3]),_0xdfa5x3=$(wishlistButtonClass+ _0xd3ea[5]+ _0xdfa5x2+ _0xd3ea[6]);_0xdfa5x3[_0xd3ea[9]](theme[_0xd3ea[8]][_0xd3ea[7]]);_0xdfa5x3[_0xd3ea[12]](_0xd3ea[10],theme[_0xd3ea[8]][_0xd3ea[11]]);wishlistObject[_0xd3ea[14]](wishlistObject[_0xd3ea[13]](_0xdfa5x2),1);localStorage[_0xd3ea[17]](_0xd3ea[15],JSON[_0xd3ea[16]](wishlistObject));$(this)[_0xd3ea[20]](_0xd3ea[19])[_0xd3ea[18]]();$wishlistCount[_0xd3ea[22]](wishlistObject[_0xd3ea[21]]);if(wishlistObject[_0xd3ea[21]]=== 0){loadNoResult()}});loadWishlist();return {load:loadWishlist}
})()
theme.alert = (function(){
	var $alert = $('#js-global-alert'),
		$title = $('#js-global-alert .alert-heading'),
		$content = $('#js-global-alert .alert-content'),
		close = '#js-global-alert .close';
	$(document).on('click',close,function(){
		$alert.removeClass('active');
	});
	function createAlert(title,mess,time,type){
		var alertTitle = title || '',
			showTime = time || 3000,
			alertClass = type || 'alert-success';
		$alert.removeClass('alert-success').removeClass('alert-danger').removeClass('alert-warning')
		$alert.addClass(alertClass);
		$title.html(title);
		$content.html(mess);
		$alert.addClass('active');
		setTimeout(function(){
			$alert.removeClass('active');
		}, showTime); 
	}
	return{
		new:createAlert
	}
})()
theme.strings = {
	wishlistNoResult: "<h3>Sản phẩm nào của chúng tôi bạn mong muốn sở hữu nhất?</h3><p>Hãy thêm vào danh sách sản phẩm yêu thích ngay nhé!</p>",
	wishlistIcon: "<svg xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 51.997 51.997' style='enable-background:new 0 0 51.997 51.997;' xml:space='preserve'><path d='M51.911,16.242C51.152,7.888,45.239,1.827,37.839,1.827c-4.93,0-9.444,2.653-11.984,6.905c-2.517-4.307-6.846-6.906-11.697-6.906c-7.399,0-13.313,6.061-14.071,14.415c-0.06,0.369-0.306,2.311,0.442,5.478c1.078,4.568,3.568,8.723,7.199,12.013l18.115,16.439l18.426-16.438c3.631-3.291,6.121-7.445,7.199-12.014C52.216,18.553,51.97,16.611,51.911,16.242z M49.521,21.261c-0.984,4.172-3.265,7.973-6.59,10.985L25.855,47.481L9.072,32.25c-3.331-3.018-5.611-6.818-6.596-10.99c-0.708-2.997-0.417-4.69-0.416-4.701l0.015-0.101C2.725,9.139,7.806,3.826,14.158,3.826c4.687,0,8.813,2.88,10.771,7.515l0.921,2.183l0.921-2.183c1.927-4.564,6.271-7.514,11.069-7.514c6.351,0,11.433,5.313,12.096,12.727C49.938,16.57,50.229,18.264,49.521,21.261z'/></svg>",
	wishlistIconAdded: "<svg class='evo-added' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 51.997 51.997' style='enable-background:new 0 0 51.997 51.997;' xml:space='preserve'><path d='M51.911,16.242C51.152,7.888,45.239,1.827,37.839,1.827c-4.93,0-9.444,2.653-11.984,6.905c-2.517-4.307-6.846-6.906-11.697-6.906c-7.399,0-13.313,6.061-14.071,14.415c-0.06,0.369-0.306,2.311,0.442,5.478c1.078,4.568,3.568,8.723,7.199,12.013l18.115,16.439l18.426-16.438c3.631-3.291,6.121-7.445,7.199-12.014C52.216,18.553,51.97,16.611,51.911,16.242z M49.521,21.261c-0.984,4.172-3.265,7.973-6.59,10.985L25.855,47.481L9.072,32.25c-3.331-3.018-5.611-6.818-6.596-10.99c-0.708-2.997-0.417-4.69-0.416-4.701l0.015-0.101C2.725,9.139,7.806,3.826,14.158,3.826c4.687,0,8.813,2.88,10.771,7.515l0.921,2.183l0.921-2.183c1.927-4.564,6.271-7.514,11.069-7.514c6.351,0,11.433,5.313,12.096,12.727C49.938,16.57,50.229,18.264,49.521,21.261z'/></svg>",
	wishlistText: "Thêm vào yêu thích",
	wishlistTextAdded: "Đến trang sản phẩm yêu thích",
	wishlistRemove: "Xóa",
	compareNoResult: "Vui lòng chọn sản phẩm để so sánh",
	compareIcon: "<svg version='1.1' xmlns='http://www.w3.org/2000/svg' xmlns:xlink='http://www.w3.org/1999/xlink' x='0px' y='0px' viewBox='0 0 477.867 477.867' style='enable-background:new 0 0 477.867 477.867;' xml:space='preserve'><path d='M409.6,0c-9.426,0-17.067,7.641-17.067,17.067v62.344C304.667-5.656,164.478-3.386,79.411,84.479c-40.09,41.409-62.455,96.818-62.344,154.454c0,9.426,7.641,17.067,17.067,17.067S51.2,248.359,51.2,238.933c0.021-103.682,84.088-187.717,187.771-187.696c52.657,0.01,102.888,22.135,138.442,60.976l-75.605,25.207c-8.954,2.979-13.799,12.652-10.82,21.606s12.652,13.799,21.606,10.82l102.4-34.133c6.99-2.328,11.697-8.88,11.674-16.247v-102.4C426.667,7.641,419.026,0,409.6,0z'/><path d='M443.733,221.867c-9.426,0-17.067,7.641-17.067,17.067c-0.021,103.682-84.088,187.717-187.771,187.696c-52.657-0.01-102.888-22.135-138.442-60.976l75.605-25.207c8.954-2.979,13.799-12.652,10.82-21.606c-2.979-8.954-12.652-13.799-21.606-10.82l-102.4,34.133c-6.99,2.328-11.697,8.88-11.674,16.247v102.4c0,9.426,7.641,17.067,17.067,17.067s17.067-7.641,17.067-17.067v-62.345c87.866,85.067,228.056,82.798,313.122-5.068c40.09-41.409,62.455-96.818,62.344-154.454C460.8,229.508,453.159,221.867,443.733,221.867z'/></svg>",
	compareText: "So sánh",
	compareRemove: "Xóa khỏi danh sách",
	compareNotifyAdded: 'Đã thêm vào danh sách so sánh',
	compareNotifyRemoved: "Đã xóa khỏi dánh sách so sánh",
	compareNotifyMaximum: "So sánh tối đa 4 sản phẩm",
};
jQuery(document).ready(function(){
	if ($('.addThis_listSharing').length > 0){
		$(window).scroll(function(){
			if(jQuery(window).scrollTop() > 100 ) {
				jQuery('.addThis_listSharing').addClass('is-show');
			} else {
				jQuery('.addThis_listSharing').removeClass('is-show');
			}
		});
	}
});