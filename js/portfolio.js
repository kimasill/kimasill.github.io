(function($){
  $(function(){
    // Theme toggle
    var theme = localStorage.getItem('theme') || 'dark';
    $('body').attr('data-theme', theme);
    $('#theme-toggle').on('click', function(){
      var t = $('body').attr('data-theme') === 'dark' ? 'light' : 'dark';
      $('body').attr('data-theme', t);
      localStorage.setItem('theme', t);
    });

    // i18n
    var dict = {
      ko: {
        'nav.home':'Home','nav.work':'Work','nav.skills':'Skills','nav.about':'About','nav.contact':'Contact',
        'hero.title':'KIMASILL','hero.subtitle':'Game Developer / Programmer','hero.cta':'이력서 다운로드',
        'work.title':'Featured Work','filter.all':'All','filter.tools':'Tools','common.details':'자세히',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI'
      },
      en: {
        'nav.home':'Home','nav.work':'Work','nav.skills':'Skills','nav.about':'About','nav.contact':'Contact',
        'hero.title':'KIMASILL','hero.subtitle':'Game Developer / Programmer','hero.cta':'Download Resume',
        'work.title':'Featured Work','filter.all':'All','filter.tools':'Tools','common.details':'Details',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI'
      }
    };
    function applyI18n(lang){
      $('[data-i18n]').each(function(){
        var key = $(this).data('i18n');
        if(dict[lang] && dict[lang][key]){
          $(this).text(dict[lang][key]);
        }
      });
      $('#lang-toggle').text(lang==='en'?'EN':'KR');
    }
    var lang = localStorage.getItem('lang') || 'ko';
    applyI18n(lang);
    $('#lang-toggle').on('click', function(){
      lang = (lang==='ko'?'en':'ko');
      localStorage.setItem('lang', lang);
      applyI18n(lang);
    });

    // Filter buttons
    $('.filter-btn').on('click', function(){
      var filter = $(this).data('filter');
      $('.filter-btn').removeClass('active');
      $(this).addClass('active');
      if(!filter || filter === 'all'){
        $('.work-card').show();
        return;
      }
      var f = String(filter).toLowerCase();
      $('.work-card').each(function(){
        var tags = ($(this).data('tags') || '').toString().toLowerCase();
        if(tags.indexOf(f) !== -1){ $(this).show(); } else { $(this).hide(); }
      });
    });

    // IntersectionObserver for reveal and lazy video
    var io = new IntersectionObserver(function(entries){
      entries.forEach(function(entry){
        if(entry.isIntersecting){
          $(entry.target).addClass('in-view');
          var video = $(entry.target).find('video.work-video')[0];
          if(video && !video.src && video.dataset.src){
            video.src = video.dataset.src;
          }
        }
      });
    }, {threshold: 0.2});
    $('.reveal').each(function(){ io.observe(this); });

    // Hover play/pause on videos
    $(document).on('mouseenter', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.play().catch(function(){}); }
    });
    $(document).on('mouseleave', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.pause(); v.currentTime = 0; }
    });
  });
})(jQuery);
