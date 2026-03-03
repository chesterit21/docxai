import { PdfViewerComponent, Toolbar, Magnification, Navigation, LinkAnnotation, BookmarkView, ThumbnailView, Print, TextSelection, Annotation, TextSearch, FormFields, FormDesigner, PageOrganizer, Inject } from '@syncfusion/ej2-react-pdfviewer';
import { registerLicense } from '@syncfusion/ej2-base';
import { useRef } from 'react';
import { useNavigate } from 'react-router-dom';
registerLicense('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA=');

export default function Index() {
    let reff = useRef<PdfViewerComponent>(null);
      const navigate = useNavigate();
    const uri = "https://pdfobject.com/pdf/sample.pdf"

    const downloadClicked = async () => {
        if (reff.current) {
            const blob = await reff.current.saveAsBlob();
            if (!blob) return;
            const blobUrl = URL.createObjectURL(blob);
            navigate("/preview", { state: { blobUrl } });
        }
    }
    return (
        <>
        <h1>Test Page</h1>
        <button onClick={downloadClicked}>Download</button>
        <div className='control-section'>
            <PdfViewerComponent 
                ref={reff}
                id="container" 
                documentPath= {uri}
                resourceUrl = {window.location.origin + "/ej2-pdfviewer-lib"} 
                style={{ 'height': '840px' }}>

                <Inject services={[Toolbar, Magnification, Navigation, Annotation, LinkAnnotation, BookmarkView, ThumbnailView, Print, TextSelection, TextSearch, FormFields, FormDesigner, PageOrganizer]}/>
            </PdfViewerComponent>
        </div>
        </>
    )
}