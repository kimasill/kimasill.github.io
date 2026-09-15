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
        'about.p1':'게임·서버·데이터 엔진 개발 경험을 바탕으로 풀스택·AI 서비스 개발에 집중하는 개발자입니다. 한국수자원공사와 협업한 CLEARIX에서 Python·FastAPI 기반 데이터 생성 알고리즘·검증 파이프라인과 작업 관리·복구 구조를 구현했습니다. PCG 모듈, 실시간 네트워크, 딥러닝 분류 모델 개발 경험을 바탕으로 복잡한 요구사항을 구체적인 기능과 시스템으로 풀어냅니다.',
        'about.cta':'이력서 다운로드'
      },
      en: {
        'nav.home':'Home','nav.work':'Work','nav.about':'ABOUT','nav.contact':'Contact',
        'hero.title':'SUNG HYEON KIM','hero.subtitle':'Game Developer / System Architect / World Designer',
        'work.title':'FEATURED WORK','filter.all':'All','filter.tools':'Tools','common.details':'Portfolio page','common.process':'Development process',
        'proj.roguelike.title':'Roguelike Shooter','proj.roguelike.meta':'Unity · C# · Procedural Level · Wave AI',
        'about.title':'ABOUT',
        'about.p1':'A developer focused on full-stack and AI services, building on experience in game, server, and data-engine development. On CLEARIX, a collaboration with K-water, I implemented Python/FastAPI-based data generation algorithms, a validation pipeline, and a job management and recovery architecture. With hands-on experience in PCG modules, real-time networking, and deep learning classifiers, I turn complex requirements into concrete features and systems.',
        'about.cta':'Download Resume'
      }
    };
    function applyI18n(lang){
      $('[data-i18n]').each(function(){
        // attr: 점(.)이 들어간 키(common.process)는 jQuery .data()로 조회 시 깨질 수 있음
        var key = $(this).attr('data-i18n');
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
