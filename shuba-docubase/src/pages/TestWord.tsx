import {
    DocumentEditorContainerComponent, Toolbar
} from '@syncfusion/ej2-react-documenteditor';
DocumentEditorContainerComponent.Inject(Toolbar);
import { registerLicense } from '@syncfusion/ej2-base';
registerLicense('Ngo9BigBOggjHTQxAR8/V1NNaF5cXmBCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWXtdcHVVQ2FZWUBxXkFWYUA=');

export default function Index() {
    let container: DocumentEditorContainerComponent;
    function onClick() {
        container.documentEditor.open( window.location.origin + "/sample.docx");
    }
    return (
        <>
        <div>
            <button id='import' onClick={onClick}>Import</button>
            <DocumentEditorContainerComponent id="container" ref={(scope: any) => { container = scope; }}
                height={'590px'}
                serviceUrl="https://services.syncfusion.com/react/production/api/documenteditor/"
                enableToolbar={true}
            />
        </div>
        </>
    )
}