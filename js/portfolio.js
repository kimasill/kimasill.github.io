(function($){
  $(function(){
    /* scripts.min.js가 모든 header a에 preventDefault + $(href) 스크롤을 거는데,
       href가 projects/... 또는 dawnstar.html 등이면 선택자가 무효라 네비가 죽음 — proj-header만 일반 링크 동작 */
    $('header.proj-header a').off('click');

    // Theme toggle
    var theme = localStorage.getItem('theme') || 'light';
    $('body').attr('data-theme', theme);
    $('#theme-toggle').on('click', function(){
      var t = $('body').attr('data-theme') === 'dark' ? 'light' : 'dark';
      $('body').attr('data-theme', t);
      localStorage.setItem('theme', t);
    });

    // i18n
    var dict = {
      ko: {
        'nav.home':'Home','nav.work':'Work','nav.about':'ABOUT','nav.contact':'Contact',
        'hero.title':'SUNG HYEON KIM','hero.subtitle':'Game Developer / System Architect / World Designer',
        'work.title':'FEATURED WORK','filter.all':'All','filter.tools':'Tools','common.details':'포트폴리오 페이지','common.process':'개발 프로세스 페이지',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'ABOUT',
        'about.cta':'이력서 다운로드'
      },
      en: {
        'nav.home':'Home','nav.work':'Work','nav.about':'ABOUT','nav.contact':'Contact',
        'hero.title':'SUNG HYEON KIM','hero.subtitle':'Game Developer / System Architect / World Designer',
        'work.title':'FEATURED WORK','filter.all':'All','filter.tools':'Tools','common.details':'Portfolio page','common.process':'Development process',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'ABOUT',
        'about.p1':'Building on my experience in game and server development, I am expanding into full-stack and AI-powered services. My strength is understanding the user experience and designing the data processing and system architecture that support it. In CLEARIX, a collaboration with K-water, I implemented an engine that generates synthetic data from water-treatment time series and validates its quality. Using Python and FastAPI, I built the pipeline from input validation through generation, quality checks, and output. I implemented job state management, cancellation, and recovery after interruption, checked input file integrity, and designed the system to save only results that pass validation. I also documented API specifications and data formats for the UI team. Previously, I built a PCG-based level generation module and networking for a dedicated-server 2D MMORPG, and developed a deep learning classifier. Alongside commercial engines, I have built games using Java and C# libraries, designing and completing the required features and architecture myself. I turn complex requirements into implementable features and design systems with data flow and failure cases in mind. I value applying new technologies to real problems and creating structures and documentation that teammates can understand and extend.',
        'about.cta':'Download Resume'
      }
    };
    // Preserve the authored Korean introduction, including its line breaks.
    var aboutIntro = $('[data-i18n="about.p1"]').first();
    var aboutIntroHtml = aboutIntro.length ? aboutIntro.html() : null;
    function applyI18n(lang){
      $('[data-i18n]').each(function(){
        // attr: 점(.)이 들어간 키(common.process)는 jQuery .data()로 조회 시 깨질 수 있음
        var key = $(this).attr('data-i18n');
        if(key === 'about.p1' && lang === 'ko' && aboutIntroHtml !== null){
          $(this).html(aboutIntroHtml);
          return;
        }
        if(dict[lang] && key && dict[lang][key]){
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

    function normalizeYouTubeEmbedUrl(rawUrl){
      if(!rawUrl) return rawUrl;
      try{
        var url = new URL(rawUrl, window.location.origin);
        var host = url.hostname.replace(/^www\./, '');
        var videoId = '';

        if(host === 'youtu.be'){
          videoId = url.pathname.replace(/^\/+/, '').split('/')[0];
        } else if(host === 'youtube.com' || host === 'm.youtube.com' || host === 'youtube-nocookie.com'){
          if(url.pathname.indexOf('/embed/') === 0){
            return rawUrl;
          }
          if(url.pathname === '/watch'){
            videoId = url.searchParams.get('v') || '';
          }
        }

        if(!videoId) return rawUrl;

        return 'https://www.youtube-nocookie.com/embed/' + videoId +
          '?autoplay=1&mute=1&controls=0&loop=1&playlist=' + videoId +
          '&modestbranding=1&playsinline=1';
      } catch(e){
        return rawUrl;
      }
    }

    // Lazy hero video iframe injection
    var heroDiv = document.querySelector('.hero-video');
    if(heroDiv && heroDiv.dataset.videoSrc){
      var observer = new IntersectionObserver(function(es){
        es.forEach(function(e){
          if(e.isIntersecting){
            var iframe = document.createElement('iframe');
            iframe.setAttribute('src', normalizeYouTubeEmbedUrl(heroDiv.dataset.videoSrc));
            iframe.setAttribute('frameborder','0');
            iframe.setAttribute('allow','autoplay; encrypted-media');
            heroDiv.appendChild(iframe);
            observer.disconnect();
          }
        });
      }, {threshold:0.1});
      observer.observe(heroDiv);
    }

    // Hover play/pause on videos
    $(document).on('mouseenter', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.play().catch(function(){}); }
    });
    $(document).on('mouseleave', '.work-media', function(){
      var v = $(this).find('video.work-video')[0];
      if(v){ v.pause(); v.currentTime = 0; }
    });

    // OVERVIEW 이미지 마우스 오버 시 호버한 이미지 옆에만 설명 툴팁 표시 (car-classification)
    $(document).on('mouseenter', '.proj-hover-desc-item', function(){
      var desc = $(this).data('desc') || '';
      var tooltip = $(this).closest('.proj-overview-with-desc').find('.proj-hover-desc-tooltip');
      if(!tooltip.length || !desc) return;
      var r = this.getBoundingClientRect();
      var pad = 16;
      var ttW = 280;
      var ttH = 80;
      var left = r.right + pad;
      var top = r.top + (r.height / 2) - (ttH / 2);
      if(left + ttW > window.innerWidth) left = r.left - pad - ttW;
      if(top < 12) top = 12;
      if(top + ttH > window.innerHeight - 12) top = window.innerHeight - ttH - 12;
      tooltip.css({ left: left + 'px', top: top + 'px' }).text(desc).addClass('is-visible').attr('aria-hidden', 'false');
    });
    $(document).on('mouseleave', '.proj-hover-desc-item', function(){
      $(this).closest('.proj-overview-with-desc').find('.proj-hover-desc-tooltip').removeClass('is-visible').text('').attr('aria-hidden', 'true');
    });
  });
})(jQuery);
