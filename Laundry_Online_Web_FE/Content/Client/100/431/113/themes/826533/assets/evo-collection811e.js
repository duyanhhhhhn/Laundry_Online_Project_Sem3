$(document).ready(function($){
	$('.evo-btn-filter').click(function(){
		$(".left-content").toggleClass('active');
		$(".backdrop__body-backdrop___1rvky").addClass('active');
	});
	$('.aside-filter .aside-hidden-mobile .aside-item .aside-title').on('click', function(e){
		e.preventDefault();
		var $this = $(this);
		$this.parents('.aside-filter .aside-hidden-mobile .aside-item').find('.aside-content').stop().slideToggle();
		$(this).toggleClass('active')
		return false;
	});
	$('.sort-cate-left h3').on('click', function(e){
		e.preventDefault();var $this = $(this);
		$this.parents('.sort-cate-left').find('ul').stop().slideToggle();
		$(this).toggleClass('active');
		return false;
	});
});