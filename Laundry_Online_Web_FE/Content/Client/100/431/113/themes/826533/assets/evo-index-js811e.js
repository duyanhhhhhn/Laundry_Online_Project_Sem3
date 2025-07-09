var swiper = new Swiper('.evo-owl-product3', {
	slidesPerView: 4,
	spaceBetween: 15,
	slidesPerGroup: 2,
	navigation: {
		nextEl: '.swiper-button-next',
		prevEl: '.swiper-button-prev',
	},
	breakpoints: {
		300: {
			slidesPerView: 2,
			spaceBetween: 7,
		},
		500: {
			slidesPerView: 2,
			spaceBetween: 10,
		},
		640: {
			slidesPerView: 2,
			spaceBetween: 10,
		},
		768: {
			slidesPerView: 3,
			spaceBetween: 10,
		},
		1024: {
			slidesPerView: 4,
			spaceBetween: 15,
		},
		1200: {
			slidesPerView: 5,
			spaceBetween: 15,
		}
	}
});
$(".not-dqtab").each( function(e){
	var $this1 = $(this);
	var datasection = $this1.closest('.not-dqtab').attr('data-section');
	$this1.find('.tabs-title li:first-child').addClass('current');
	var view = $this1.closest('.not-dqtab').attr('data-view');
	$this1.find('.tab-content').first().addClass('current');

	var _this = $(this).find('.content .button_show_tab');
	var droptab = $(this).find('.tab-desktop');

	$(_this).click(function(){ 
		if ($(droptab).hasClass('evo-open')) {
			$(droptab).addClass('evo-close').removeClass('evo-open');
		}else {
			$(droptab).addClass('evo-open').removeClass('evo-close');
		}
		$(this).toggleClass('active');
	});
	$this1.find('.tabs-title.ajax li').click(function(){
		$(droptab).addClass('evo-close').removeClass('evo-open');
		$('.content .button_show_tab').removeClass('active');
		var $this2 = $(this),
			tab_id = $this2.attr('data-tab'),
			url = $this2.attr('data-url');
		var etabs = $this2.closest('.e-tabs');
		etabs.find('.tab-viewall').attr('href',url);
		etabs.find('.tabs-title li').removeClass('current');
		etabs.find('.tab-content').removeClass('current');
		$this2.addClass('current');
		etabs.find("."+tab_id).addClass('current');
		if(!$this2.hasClass('has-content')){
			$this2.addClass('has-content');		
			getContentTab(url,"."+ datasection+" ."+tab_id,view);
		}
	});
});
$('.not-dqtab .next').click(function(e){
	var count = 0
	$(this).parents('.content').find('.tab-content').each(function(e){
		count +=1;
	})
	var str = $(this).parent().find('.tab-titlexs').attr('data-tab'),
		res = str.replace("tab-", ""),
		datasection = $(this).closest('.not-dqtab').attr('data-section');
	res = Number(res);
	if(res < count){
		var current = res + 1;
	}else{
		var current = 1;
	}
	action(current,datasection);
})
$('.not-dqtab .prev').click(function(e){
	var count = 0
	$(this).parents('.content').find('.tab-content').each(function(e){
		count +=1;
	})
	var str = $(this).parent().find('.tab-titlexs').attr('data-tab'),
		res = str.replace("tab-", ""),
		datasection = $(this).closest('.not-dqtab').attr('data-section'),
		res = Number(res);	
	if(res > 1){
		var current = res - 1;
	}else{
		var current = count;
	}
	action(current,datasection);
})
function action(current,datasection,view){
	$('.'+datasection+' .tab-titlexs').attr('data-tab','tab-'+current);
	var text = '',
		url = '',
		tab_id='';
	$('.'+datasection+' ul.tabs.tabs-title.hidden-xs li').each(function(e){
		if($(this).attr('data-tab') == 'tab-'+current){
			var $this3 = $(this);
			title = $this3.find('span').text();
			url = $this3.attr('data-url');
			tab_id = $this3.attr('data-tab');
			if(!$this3.hasClass('has-content')){
				$this3.addClass('has-content');	
				getContentTab(url,"."+ datasection+" ."+tab_id,view);				
			}
		}
	})
	$("."+ datasection+" .tab-titlexs span").text(title);
	$("."+ datasection+" .tab-content").removeClass('current');
	$("."+ datasection+" .tab-"+current).addClass('current');
}
function getContentTab(url,selector,view){
	if(view == 'grid_4'){
		url = url+"?view=ajaxload4";
	}else{
		url = url+"?view=ajaxload";
	}
	var dataLgg = $(selector).parent().find('.tab-1 .owl-carousel').attr('data-lgg-items');
	var loadding = '<div class="loading"><div class="chasing-dots"><div></div><div></div><div></div></div></div>';
	$.ajax({
		type: 'GET',
		url: url,
		beforeSend: function() {
			$(selector).html(loadding);
		},
		success: function(data) {
			var content = $(data);
			$(selector).html(content.html());
			awe_lazyloadImage();
			ajaxCarousel(selector);
			$('.evo-owl-product2 .add_to_cart').bind( 'click', addToCart );
			$('.evo-owl-product3 .add_to_cart').bind( 'click', addToCart );
			if(window.BPR)
				return window.BPR.initDomEls(), window.BPR.loadBadges();
		},
		dataType: "html"
	});
}
function ajaxCarousel(selector){
	$(selector+' .evo-owl-product3').each( function(){
		var swiper = new Swiper('.evo-owl-product3', {
			slidesPerView: 4,
			spaceBetween: 15,
			slidesPerGroup: 2,
			navigation: {
				nextEl: '.swiper-button-next',
				prevEl: '.swiper-button-prev',
			},
			breakpoints: {
				300: {
					slidesPerView: 2,
					spaceBetween: 7,
				},
				500: {
					slidesPerView: 2,
					spaceBetween: 10,
				},
				640: {
					slidesPerView: 2,
					spaceBetween: 10,
				},
				768: {
					slidesPerView: 3,
					spaceBetween: 10,
				},
				1024: {
					slidesPerView: 4,
					spaceBetween: 15,
				},
				1200: {
					slidesPerView: 5,
					spaceBetween: 15,
				}
			}
		});
	})
}